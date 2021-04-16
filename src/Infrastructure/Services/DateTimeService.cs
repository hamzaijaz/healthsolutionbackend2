using System;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;

namespace CapitalRaising.RightsIssues.Service.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}