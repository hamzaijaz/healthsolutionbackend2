using System.Collections.Generic;

namespace MyHealthSolution.Service.Infrastructure.ServiceBus
{
    public interface IServiceBusConfiguration
    {
        public string DefaultConnectionString {get;}
        public Dictionary<string, string> OtherConnectionStrings {get;}
    }
}