using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyHealthSolution.Service.Application.Common.Interfaces
{
    public interface ICsvFileBuilder
    {
        Task<byte[]> BuildFileAsync<TRecord>(IEnumerable<TRecord> records);
    }
}
