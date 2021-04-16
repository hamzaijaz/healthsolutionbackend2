using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;

namespace CapitalRaising.RightsIssues.Service.Infrastructure.Context
{
    [ExcludeFromCodeCoverage]
    public class MutableCallContext : ICallContext
    {
        public Guid CorrelationId { get; set; }
        public string UserName { get; set; }
        public string AuthenticationType { get; set; }
        public string FunctionName { get; set; }
        public IDictionary<string, string> AdditionalProperties { get; } = new Dictionary<string, string>();
        public string OriginatingUsername { get; set; }
        public string OriginatingUserId { get; set; }
        public string IPAddress { get; set; }
        public string OriginalSubscriptionKey { get; set; }
        public Guid? IdempotencyKey { get; set; }
    }
}
