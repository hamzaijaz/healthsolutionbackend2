using System;
using System.Threading.Tasks;
using MediatR;

namespace MyHealthSolution.Service.FunctionApp
{
    public interface IFunctionMediator
    {
        Task ExecuteAsync<TRequest>(Guid invocationId,
                                                    string functionName,
                                                    TRequest request) 
                                                    where TRequest : IRequest;

        Task<TResponse> ExecuteAsync<TRequest, TResponse>(Guid invocationId,
                                                    string functionName,
                                                    TRequest request) 
                                                    where TRequest : IRequest<TResponse>;
    }
}