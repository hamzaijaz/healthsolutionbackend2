using System.Threading.Tasks;
using MyHealthSolution.Service.Application.Common.Models;

namespace MyHealthSolution.Service.Application.Common.Interfaces
{
    public interface IRequestAuditEnricher<TRequest>
    {
        Task EnrichAudit(AuditEntry entry, TRequest request);
    }
}