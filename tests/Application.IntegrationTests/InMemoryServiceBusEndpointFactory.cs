using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using System.Collections.Concurrent;

namespace Application.IntegrationTests
{
    internal class InMemoryServiceBusEndpointFactory : IBusEndpointFactory
    {
        private ConcurrentDictionary<string, InMemoryServiceBusEndpoint> endpoints = new ConcurrentDictionary<string, InMemoryServiceBusEndpoint>();

        public ConcurrentDictionary<string, InMemoryServiceBusEndpoint> Endpoints { get => endpoints; }

        public IBusEndpoint Create(string queueOrTopicName)
        {
            return this.endpoints.GetOrAdd(queueOrTopicName, new InMemoryServiceBusEndpoint(queueOrTopicName));
        }

        public IBusEndpoint Create<TPayload>() where TPayload : class
        {
            return this.endpoints.GetOrAdd(typeof(TPayload).FullName, new InMemoryServiceBusEndpoint(typeof(TPayload).FullName));
        }

        public void ResetState()
        {
            // Clear all messages for each endpoint
            foreach(var endpoint in this.endpoints)
            {
                endpoint.Value.Messages.Clear();
            }
        }
    }
}