using System;

namespace CapitalRaising.Administration.Service.Application.Issuers.Messages
{
    /// <summary>
    /// Issuer Changed Event
    /// </summary>
    public class IssuerChangedEvent
    {
        /// <summary>
        /// End point name
        /// </summary>
        public const string EndpointName = "capitalraising.administration.messages.issuerchangedevent";

        public Guid IssuerKey { get; set; }
        public string Name { get; set; }
        public string Ticker { get; set; }
        public string Exchange { get; set; }
        public string BusinessNumber { get; set; }
        public string ContactName { get; set; }
        public string ContactPositionTitle { get; set; }
        public string ContactEmail { get; set; }
        public string ContactMobileNumber { get; set; }
        public string PreferredComms { get; set; }
        public string TimeZoneId { get; set; }
    }
}