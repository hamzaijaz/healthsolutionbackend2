using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MyHealthSolution.Service.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace MyHealthSolution.Service.FunctionApp
{
    [ExcludeFromCodeCoverage]
    public static class Extensions
    {
        public static Task<IActionResult> ToTask(this IActionResult result)
        {
            return Task.FromResult(result);
        }
    }
}