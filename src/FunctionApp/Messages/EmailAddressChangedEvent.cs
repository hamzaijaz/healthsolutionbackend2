using System;

namespace CapitalRaising.RightsIssues.Service.Application.EmailAddresses.Messages
{
    /// <summary>
    /// Email address Changed Event
    /// </summary>
    public class EmailAddressChangedEvent
    {
        /// <summary>
        /// End point name
        /// </summary>
        public const string EndpointName = "capitalraising.administration.messages.emailaddresschangedevent";

        public Guid IssuerKey { get; set; }
        public Guid EmailAddressKey { get; set; }
        public string EmailAddress { get; set; }
        public string Action { get; set; }
    }
}