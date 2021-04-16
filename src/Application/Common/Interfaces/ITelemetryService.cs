using CapitalRaising.RightsIssues.Service.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface ITelemetryService
    {
        void SendAvailabilityTelemetry(AvailabilityTelemetry availability);
    }
}
