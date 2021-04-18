using System;

namespace MyHealthSolution.Service.FunctionApp.Messages
{
    public class PortalDeploymentCompletedEvent
    {
        public const string EndpointName = "capitalraising.administration.messages.portaldeploymentcompletedevent";

        public Guid EventKey { get; set; }
        public Guid IssuerKey { get; set; }
        public string PortalPrefix { get; set; }
        public string Version { get; set; }
        public DateTime CompletedAtUtc { get; set; }
        public string CompletedBy { get; set; }
        public int DeploymentId { get; set; }
    }
}