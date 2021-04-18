using System;
using MyHealthSolution.Service.Application.Common.Interfaces;

namespace MyHealthSolution.Service.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}