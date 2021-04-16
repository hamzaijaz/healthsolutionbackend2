using System;

namespace CapitalRaising.RightsIssues.Service.FunctionApp.Messages
{
    public class LetterGenerationCompletedEvent
    {
        /// <summary>
        /// End point name
        /// </summary>
        public const string EndpointName = "capitalraising.communication.messages.lettergenerationcompletedevent";

        public Guid EventKey { get; set; }
        public Guid IssuerKey { get; set; }
        public DateTime CompletedAtUtc { get; set; }
        public string CompletedBy {get;set;}
        public string CorrespondenceType {get;set;}
    }
}
