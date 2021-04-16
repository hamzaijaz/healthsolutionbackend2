using CapitalRaising.RightsIssues.Service.Application.HealthCheck.Commands;
using Microsoft.Azure.WebJobs;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CapitalRaising.RightsIssues.Service.FunctionApp.TimerTriggers
{
    [ExcludeFromCodeCoverage]
    public class HealthCheckFunctions
    {
        private readonly IFunctionMediator _functionMediator;

        public HealthCheckFunctions(IFunctionMediator functionMediator)
        {
            _functionMediator = functionMediator;
        }

        [FunctionName("ExecuteHealthCheck")]
        public async Task ExecuteHealthCheck([TimerTrigger("%HealthCheckCRON%", RunOnStartup = true)]TimerInfo myTimer, 
            ExecutionContext context)
        {
            var command = new ExecuteHealthCheckCommand();
            await _functionMediator.ExecuteAsync(context.InvocationId, context.FunctionName, command);
        }
    }
}
