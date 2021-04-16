using System;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface IDateTime
    {
        DateTime Now { get; }

        DateTime UtcNow { get; }
    }
}