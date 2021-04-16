using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapitalRaising.RightsIssues.Service.FunctionApp
{
    public interface IHttpFunctionMediator
    {
        Task<IActionResult> ExecuteAsync<TRequest, TResponse>(Guid invocationId,
                                                                string functionName,
                                                                HttpRequest httpRequest,
                                                                TRequest request,
                                                                Func<TResponse, Task<IActionResult>> resultMethod = null)
            where TRequest : IRequest<TResponse>;
    }
}