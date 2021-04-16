using System;
using System.Threading.Tasks;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Application.IntegrationTests
{
    internal class InMemoryServiceBusEndpoint : IBusEndpoint
    {
        private readonly string endpointName;
        private ConcurrentDictionary<Type, List<object>> messages = new ConcurrentDictionary<Type, List<object>>();

        public ConcurrentDictionary<Type, List<object>> Messages { get => messages; }

        public InMemoryServiceBusEndpoint(string endpointName)
        {
            this.endpointName = endpointName ?? throw new ArgumentNullException(nameof(endpointName));
        }

        public Task SendAsync<TPayload>(TPayload payload) where TPayload : class
        {
            messages.AddOrUpdate(typeof(TPayload), 
                                (type) => 
                                { 
                                    return new List<object>{payload}; 
                                }, 
                                (type, objects) => 
                                { 
                                    objects.Add(payload);
                                    return objects;
                                });
                                
            return Task.CompletedTask;
        }
    }
}