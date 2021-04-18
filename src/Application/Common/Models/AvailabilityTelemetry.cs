using System;
using System.Collections.Generic;
using System.Text;

namespace MyHealthSolution.Service.Application.Common.Models
{
    public class AvailabilityTelemetry
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
    }
}
