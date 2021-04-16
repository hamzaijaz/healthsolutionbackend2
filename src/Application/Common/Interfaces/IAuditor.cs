using System.Threading.Tasks;
using CapitalRaising.RightsIssues.Service.Application.Common.Models;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface IAuditor
    {
         Task AddAsync(Models.Audit record);
    }
}