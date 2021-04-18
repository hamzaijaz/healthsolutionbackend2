using MyHealthSolution.Service.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyHealthSolution.Service.Application.Common.Interfaces
{
    public interface ITelemetryService
    {
        void SendAvailabilityTelemetry(AvailabilityTelemetry availability);
    }
}
