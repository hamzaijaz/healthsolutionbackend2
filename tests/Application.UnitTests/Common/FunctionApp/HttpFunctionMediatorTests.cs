using System;
using System.Threading;
using System.Threading.Tasks;
using CapitalRaising.RightsIssues.Service.Application.Common.Exceptions;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using CapitalRaising.RightsIssues.Service.FunctionApp;
using CapitalRaising.RightsIssues.Service.Infrastructure.Context;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Application.UnitTests.Common.FunctionApp
{
    public class HttpFunctionMediatorTests
    {
        private readonly DefaultHttpContext httpContext;

        public HttpFunctionMediatorTests()
        {
            httpContext = new DefaultHttpContext();
        }

        [Fact]
        public async Task EnsureIncomingIdentityAndRequestHeaders_FlowToCallContext()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new Response());

            var idempotencyKey = Guid.NewGuid();
            var invocationId = Guid.NewGuid();
            var functionName = nameof(EnsureIncomingIdentityAndRequestHeaders_FlowToCallContext);
            var ipAddress = "1.1.1.1";
            var apiKey = "ABCD";

            var callContext = new MutableCallContext();
            
            httpContext.User = new System.Security.Claims.ClaimsPrincipal
            (
                new System.Security.Principal.GenericIdentity("joe", "special")
            );
            httpContext.Request.Headers.Add("X-Forwarded-For", new Microsoft.Extensions.Primitives.StringValues(ipAddress));
            httpContext.Request.Headers.Add("X-API-KEY", new Microsoft.Extensions.Primitives.StringValues(apiKey));
            httpContext.Request.Headers.Add("OriginatingUserId", new Microsoft.Extensions.Primitives.StringValues("987654321"));
            httpContext.Request.Headers.Add("OriginatingUsername", new Microsoft.Extensions.Primitives.StringValues("JoeBloggs"));
            httpContext.Request.Headers.Add("Idempotency-Key", new Microsoft.Extensions.Primitives.StringValues(idempotencyKey.ToString()));
            
            var sut = new HttpFunctionMediator(mockMediator.Object, callContext);

            // act
            IActionResult result = await sut.ExecuteAsync<Request, Response>(invocationId, functionName, httpContext.Request, new Request(), null);
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()));

            result.Should().BeOfType<OkObjectResult>();

            callContext.AuthenticationType.Should().Be("special");
            callContext.CorrelationId.Should().Be(invocationId);
            callContext.FunctionName.Should().Be(functionName);
            callContext.IPAddress.Should().Be(ipAddress);
            callContext.OriginalSubscriptionKey.Should().Be(apiKey);
            callContext.OriginatingUserId.Should().Be("987654321");
            callContext.OriginatingUsername.Should().Be("JoeBloggs");
            callContext.UserName.Should().Be("joe");
            callContext.IdempotencyKey.Should().Be(idempotencyKey);
        }

        [Fact]
        public async Task WhenNoRequestHeadersArePresent_EnsureNoExceptionsOccur()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new Response());

            var invocationId = Guid.NewGuid();
            var functionName = nameof(WhenNoRequestHeadersArePresent_EnsureNoExceptionsOccur);

            // Do not add any request headers

            var sut = new HttpFunctionMediator(mockMediator.Object, new MutableCallContext());

            // act
            IActionResult result = await sut.ExecuteAsync<Request, Response>(invocationId, functionName, httpContext.Request, new Request(), null);
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()));
            result.Should().BeOfType<OkObjectResult>();
        }


        [Fact]
        public async Task WhenNotFoundExceptionOccurs_ShouldReturn_NotFoundObjectResult()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new NotFoundException("name", "someValue"));

            var invocationId = Guid.NewGuid();
            var functionName = nameof(WhenNotFoundExceptionOccurs_ShouldReturn_NotFoundObjectResult);
            var sut = new HttpFunctionMediator(mockMediator.Object, Mock.Of<ICallContext>());

            // act
            IActionResult result = await sut.ExecuteAsync<Request, Response>(invocationId, functionName, httpContext.Request, new Request(), null);
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()));

            result.Should().BeOfType<NotFoundObjectResult>();
            result.As<NotFoundObjectResult>().Value.Should().BeOfType<ProblemDetails>();
        }

        [Fact]
        public async Task WhenUnhandledExceptionOccurs_ShouldReturn_InternalServerErrorObjectResult()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new NullReferenceException());

            var invocationId = Guid.NewGuid();
            var functionName = nameof(WhenUnhandledExceptionOccurs_ShouldReturn_InternalServerErrorObjectResult);
            var sut = new HttpFunctionMediator(mockMediator.Object, Mock.Of<ICallContext>());

            // act
            IActionResult result = await sut.ExecuteAsync<Request, Response>(invocationId, functionName, httpContext.Request, new Request(), null);
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()));

            result.Should().BeOfType<ObjectResult>();
            result.As<ObjectResult>().StatusCode.Should().Be(500);
            result.As<ObjectResult>().Value.Should().BeOfType<ProblemDetails>();
        }

        [Fact]
        public async Task WhenDuplicateItemExceptionOccurs_ShouldReturn_ConflictObjectResult()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new DuplicateItemException("exception occurred."));

            var invocationId = Guid.NewGuid();
            var functionName = nameof(WhenDuplicateItemExceptionOccurs_ShouldReturn_ConflictObjectResult);
            var sut = new HttpFunctionMediator(mockMediator.Object, Mock.Of<ICallContext>());

            // act
            IActionResult result = await sut.ExecuteAsync<Request, Response>(invocationId, functionName, httpContext.Request, new Request(), null);
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()));

            result.Should().BeOfType<ConflictObjectResult>();
            result.As<ConflictObjectResult>().Value.Should().BeOfType<ProblemDetails>();
        }

        [Fact]
        public async Task WhenBadRequestExceptionOccurs_ShouldReturn_BadRequestObjectResult()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new BadRequestException("exception occurred."));

            var invocationId = Guid.NewGuid();
            var functionName = nameof(WhenBadRequestExceptionOccurs_ShouldReturn_BadRequestObjectResult);
            var sut = new HttpFunctionMediator(mockMediator.Object, Mock.Of<ICallContext>());

            // act
            IActionResult result = await sut.ExecuteAsync<Request, Response>(invocationId, functionName, httpContext.Request, new Request(), null);
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()));

            result.Should().BeOfType<BadRequestObjectResult>();
            result.As<BadRequestObjectResult>().Value.Should().BeOfType<ProblemDetails>();
        }

        [Fact]
        public async Task WhenValidationExceptionOccurs_ShouldReturn_BadRequestObjectResult()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new ValidationException());

            var invocationId = Guid.NewGuid();
            var functionName = nameof(WhenValidationExceptionOccurs_ShouldReturn_BadRequestObjectResult);
            var sut = new HttpFunctionMediator(mockMediator.Object, Mock.Of<ICallContext>());

            // act
            IActionResult result = await sut.ExecuteAsync<Request, Response>(invocationId, functionName, httpContext.Request, new Request(), null);
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()));

            result.Should().BeOfType<BadRequestObjectResult>();
            result.As<BadRequestObjectResult>().Value.Should().NotBeOfType<ProblemDetails>().And.BeAssignableTo<ProblemDetails>();
        }

        [Fact]
        public async Task WhenUsingCustomResultDelegate_ShouldReturn_ExpectedCustomResult()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new Response());

            var invocationId = Guid.NewGuid();
            var functionName = nameof(WhenUsingCustomResultDelegate_ShouldReturn_ExpectedCustomResult);
            var sut = new HttpFunctionMediator(mockMediator.Object, Mock.Of<ICallContext>());

            Func<Response, Task<IActionResult>> resultFunc = (r) => new UnprocessableEntityObjectResult("error").ToTask();

            // act
            IActionResult result = await sut.ExecuteAsync<Request, Response>(invocationId, functionName, httpContext.Request, new Request(), resultFunc);
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()));
            
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
            result.As<UnprocessableEntityObjectResult>().Value.Should().Be("error");
        }

        private class Request : IRequest<Response>
        {
        }

        private class Response
        {
        }
    }
}