using System;
using System.Threading.Tasks;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using MediatR;

namespace CapitalRaising.RightsIssues.Service.FunctionApp
{
    /// <summary>
    /// Represents a class which acts as a mediator between the function app and the application.
    /// Initializes the call context prior to using mediator to dispatch requests to the application.
    /// </summary>
    public class GenericFunctionMediator : IFunctionMediator
    {
        private readonly ICallContext context;
        private readonly IMediator mediator;

        public GenericFunctionMediator(IMediator mediator, ICallContext context)
        {
            this.mediator = mediator;
            this.context = context;
        }

        public async Task ExecuteAsync<TRequest>(Guid invocationId,
                                                    string functionName,
                                                    TRequest request)
            where TRequest : IRequest
        {

            this.context.CorrelationId = invocationId;
            this.context.FunctionName = functionName;

            await mediator.Send(request);
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(Guid invocationId, string functionName, TRequest request) 
            where TRequest : IRequest<TResponse>
        {
            this.context.CorrelationId = invocationId;
            this.context.FunctionName = functionName;

            return await mediator.Send(request);
        }
    }
}