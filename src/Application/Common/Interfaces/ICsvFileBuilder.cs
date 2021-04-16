using System.Collections.Generic;
using System.Threading.Tasks;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface ICsvFileBuilder
    {
        Task<byte[]> BuildFileAsync<TRecord>(IEnumerable<TRecord> records);
    }
}
