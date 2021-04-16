using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CapitalRaising.RightsIssues.Service.Infrastructure.Telemetry
{
    public class TelemetryService: ITelemetryService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IConfiguration _configuration;

        public TelemetryService(
            IConfiguration configuration,
            TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
            _configuration = configuration;
        }

        public void SendAvailabilityTelemetry(Application.Common.Models.AvailabilityTelemetry availability)
        {
            var operationid = Guid.NewGuid().ToString("N");
            var azureTelemetry = new Microsoft.ApplicationInsights.DataContracts.AvailabilityTelemetry
            {
                Id = operationid,
                Name = availability.Name,
                RunLocation = _configuration.GetValue<string>("REGION_NAME"),
                Success = availability.Success,
                Timestamp = availability.Timestamp,
                Duration = availability.Duration,
                Message = availability.Message
            };
            _telemetryClient.TrackAvailability(azureTelemetry);
            _telemetryClient.Flush();
        }
    }
}
