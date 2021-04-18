using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyHealthSolution.Service.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using MyHealthSolution.Service.Application.Common.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyHealthSolution.Service.Application.HealthCheck.Commands
{
    public class ExecuteHealthCheckCommand : IRequest
    {
        public class ExecuteHealthCheckHandler : AsyncRequestHandler<ExecuteHealthCheckCommand>
        {
            private readonly IConfiguration _configuration;
            private readonly ITelemetryService _telemetryService;
            private readonly IEnumerable<IHealthCheck> _healthChecks;

            public ExecuteHealthCheckHandler(
                IConfiguration configuration,
                ITelemetryService telemetryService,
                IEnumerable<IHealthCheck> healthChecks)
            {
                _configuration = configuration;
                _telemetryService = telemetryService;
                _healthChecks = healthChecks;
            }

            protected override async Task Handle(ExecuteHealthCheckCommand request, CancellationToken cancellationToken)
            {
                var applicationName = _configuration.GetValue<string>("ApplicationFullName");
                var slotName = _configuration.GetValue<string>("APPSETTING_WEBSITE_SLOT_NAME");
                var availabilityName = slotName == null ? applicationName : $"{applicationName}-{slotName}";

                var availability = new AvailabilityTelemetry
                {
                    Name = availabilityName,
                    Success = true,
                    Message = "Success"
                };
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                foreach (var healthCheck in _healthChecks)
                {
                    var context = new HealthCheckContext()
                    {
                        Registration = new HealthCheckRegistration("", healthCheck, null, Enumerable.Empty<string>())
                    };
                    try
                    {
                        var healthCheckResult = await healthCheck.CheckHealthAsync(context, cancellationToken);
                        if (healthCheckResult.Status != HealthStatus.Healthy)
                        {
                            availability.Success = false;
                            availability.Message = healthCheck.GetType().Name + ":" + healthCheckResult.Exception?.Message;
                            break;
                        }
                    }
                    catch(Exception ex)
                    {
                        availability.Success = false;
                        availability.Message = healthCheck.GetType().Name + ":" + ex.Message;
                        break;
                    }
                }

                stopwatch.Stop();
                availability.Duration = stopwatch.Elapsed;
                availability.Timestamp = DateTime.UtcNow;
                _telemetryService.SendAvailabilityTelemetry(availability);
            }
        }
    }
}