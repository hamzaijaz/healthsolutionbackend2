using System;

namespace MyHealthSolution.Service.Application.Custodians.Messages
{
    /// <summary>
    /// Custodian Changed Event
    /// </summary>
    public class CustodianChangedEvent
    {
        /// <summary>
        /// End point name
        /// </summary>
        public const string EndpointName = "capitalraising.administration.messages.custodianchangedevent";

        public Guid CustodianKey { get; set; }
        public string Name { get; set; }
        public string LegalEntityCode { get; set; }
    }
}