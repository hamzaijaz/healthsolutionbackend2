using System;

namespace  CapitalRaising.RightsIssues.Service.FunctionApp.Models
{
    public class StageRequest
    {
        public Guid IssuerKey {get; set;}
        public Guid EventKey {get; set;}
    }
}