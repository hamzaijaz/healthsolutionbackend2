using System;

namespace MyHealthSolution.Service.FunctionApp.Messages
{
    public class SentEmailsCompletedEvent
    {
        public const string EndpointName = "capitalraising.communication.messages.sentemailscompletedevent";
        public Guid IssuerKey { get; set; }
        public Guid EventKey { get; set; }
        public string EmailType { get; set; }
        public DateTime CompletedAtUtc { get; set; }
        public string CompletedBy { get; set; }
    }
}
