using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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