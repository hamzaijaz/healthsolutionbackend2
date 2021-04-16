using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace CapitalRaising.RightsIssues.Service.Infrastructure.ServiceBus
{
    public interface IMessageEnricher
    {
        Task EnrichAsync(Message message);
    }
}