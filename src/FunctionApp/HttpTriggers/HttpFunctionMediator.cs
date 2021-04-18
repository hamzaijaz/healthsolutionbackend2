using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyHealthSolution.Service.Application.Common.Exceptions;
using MyHealthSolution.Service.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace MyHealthSolution.Service.FunctionApp
{
    /// <summary>
    /// Represents a class which acts as a mediator between the Http Function App and the application.
    /// Initializes the call context prior to using mediator to dispatch requests to the application.
    /// Exceptions are also enriched and translated back into their HTTP equivalent.
    /// </summary>
    public class HttpFunctionMediator : IHttpFunctionMediator
    {
        private readonly IMediator mediator;
        private readonly ICallContext context;
        private const string XApiKey = "X-API-KEY";
        private const string OriginatingUsername = "OriginatingUsername";
        private const string OriginatingUserId = "OriginatingUserId";
        private const string IpAddress = "X-Forwarded-For";
        private const string IdempotencyKey = "Idempotency-Key";

        public HttpFunctionMediator(IMediator mediator, ICallContext context)
        {
            this.mediator = mediator;
            this.context = context;
        }

        public async Task<IActionResult> ExecuteAsync<TRequest, TResponse>(Guid invocationId,
                                                                                string functionName,
                                                                                HttpRequest httpRequest,
                                                                                TRequest request,
                                                                                Func<TResponse, Task<IActionResult>> resultMethod = null)
            where TRequest : IRequest<TResponse>
        {
            try
            {
                // Populate the context with information. This can be used by injecting the call context into any class
                this.context.CorrelationId = invocationId;
                this.context.FunctionName = functionName;
                this.context.UserName = httpRequest.HttpContext.User?.Identity?.Name;
                this.context.AuthenticationType = httpRequest.HttpContext.User?.Identity?.AuthenticationType;
                this.context.OriginalSubscriptionKey = GetHeaderInfo(httpRequest, XApiKey);
                this.context.IPAddress = GetHeaderInfo(httpRequest, IpAddress);
                this.context.OriginatingUsername = GetHeaderInfo(httpRequest, OriginatingUsername);
                this.context.OriginatingUserId = GetHeaderInfo(httpRequest, OriginatingUserId);

                var idempotencyKeyString = GetHeaderInfo(httpRequest, IdempotencyKey);

                if(!string.IsNullOrWhiteSpace( idempotencyKeyString ))
                {
                    Guid idempotencyKey = Guid.NewGuid();
                    if(Guid.TryParse(idempotencyKeyString,out idempotencyKey))
                    {
                        this.context.IdempotencyKey = idempotencyKey;
                    }
                }

                var response = await mediator.Send(request);

                if (resultMethod != null)
                    return await resultMethod(response);

                return new OkObjectResult(response);
            }
            catch (ValidationException validationException)
            {
                // NOTE: ValidationProblemDetails ctor(Dictionary<string, string>) is part of MvC.Core 2.2 so an explicit reference was added to pull in this class because Azure Functions uses v2.1
                var details = new CustomValidationProblemDetails(validationException.Errors)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };
                return new BadRequestObjectResult(details);
            }
            catch (NotFoundException notFoundException)
            {
                var details = new ProblemDetails()
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "The specified resource was not found.",
                    Detail = notFoundException.Message
                };

                return new NotFoundObjectResult(details);
            }
            catch (DuplicateItemException duplicateException)
            {
                var details = new ProblemDetails()
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    Title = "The request could not be completed due to a conflict.",
                    Detail = duplicateException.Message
                };

                return new ConflictObjectResult(details);
            }
            catch (BadRequestException badRequestException)
            {
                var details = new ProblemDetails()
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",

                    Title = "Bad Request",
                    Detail = badRequestException.Message
                };

                return new BadRequestObjectResult(details);
            }

            catch (Exception)
            {
                var details = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while processing your request.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

                return new ObjectResult(details)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        private string GetHeaderInfo(HttpRequest httpRequest, string key)
        {
            StringValues keyvalue;
            httpRequest.Headers.TryGetValue(key, out keyvalue);
            return keyvalue.ToString() ?? "";
        }

        // NOTE: The following classes are required to customise the ValidationProblemDetails structure returned by the APIs
        // Unfortunetly, Microsoft decided to use a custom JSON converter for ProblemDetails and ValidationProblemDetails therefore
        // we have to replicate this behavior to ensure the correct response is returned because the classes are internal and not available. 
        // In addition Azure Functions use an older version of the MvC library (v2.1) which uses NewtonSoft.Json rather than System.Text.Json. 
        // Once Azure Functions upgrade to v3.1 or later, we can hopefully revisit this code and reduce it.

        [Newtonsoft.Json.JsonConverter(typeof(CustomValidationProblemDetailsConverter))]
        private class CustomValidationProblemDetails : ProblemDetails
        {
            public CustomValidationProblemDetails()
            {

            }
            public CustomValidationProblemDetails(IDictionary<string, string[]> errors)
            {
                this.Title = "One or more validation errors occured.";
                Errors = errors.Select(e => new Error { Id = e.Key, Values = e.Value }).ToArray();
            }

            public Error[] Errors { get; set; }

            public class Error
            {
                public string Id {get;set;}
                public string[] Values {get;set;}
            }
        }

        private sealed class CustomValidationProblemDetailsConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(CustomValidationProblemDetails);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                var annotatedProblemDetails = serializer.Deserialize<AnnotatedCustomValidationProblemDetails>(reader);
                if (annotatedProblemDetails == null)
                {
                    return null;
                }

                var problemDetails = (CustomValidationProblemDetails)existingValue ?? new CustomValidationProblemDetails();
                annotatedProblemDetails.CopyTo(problemDetails);

                return problemDetails;
            }

            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                var problemDetails = (CustomValidationProblemDetails)value;
                var annotatedProblemDetails = new AnnotatedCustomValidationProblemDetails(problemDetails);

                serializer.Serialize(writer, annotatedProblemDetails);
            }
        }

        private class AnnotatedCustomValidationProblemDetails : AnnotatedProblemDetails
        {
            /// <remarks>
            /// Required for JSON.NET deserialization.
            /// </remarks>
            public AnnotatedCustomValidationProblemDetails() { }

            public AnnotatedCustomValidationProblemDetails(CustomValidationProblemDetails problemDetails)
                : base(problemDetails)
            {
                Errors = problemDetails.Errors;
            }

            [JsonProperty(PropertyName = "errors")]
            public CustomValidationProblemDetails.Error[] Errors { get; }

            public void CopyTo(CustomValidationProblemDetails problemDetails)
            {
                base.CopyTo(problemDetails);

                problemDetails.Errors = Errors;
            }
        }

        private class AnnotatedProblemDetails
        {
            /// <remarks>
            /// Required for JSON.NET deserialization.
            /// </remarks>
            public AnnotatedProblemDetails() { }

            public AnnotatedProblemDetails(ProblemDetails problemDetails)
            {
                Detail = problemDetails.Detail;
                Instance = problemDetails.Instance;
                Status = problemDetails.Status;
                Title = problemDetails.Title;
                Type = problemDetails.Type;

                foreach (var kvp in problemDetails.Extensions)
                {
                    Extensions[kvp.Key] = kvp.Value;
                }
            }

            [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
            public int? Status { get; set; }

            [JsonProperty(PropertyName = "detail", NullValueHandling = NullValueHandling.Ignore)]
            public string Detail { get; set; }

            [JsonProperty(PropertyName = "instance", NullValueHandling = NullValueHandling.Ignore)]
            public string Instance { get; set; }

            [JsonExtensionData]
            public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>(StringComparer.Ordinal);

            public void CopyTo(ProblemDetails problemDetails)
            {
                problemDetails.Type = Type;
                problemDetails.Title = Title;
                problemDetails.Status = Status;
                problemDetails.Instance = Instance;
                problemDetails.Detail = Detail;

                foreach (var kvp in Extensions)
                {
                    problemDetails.Extensions[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}