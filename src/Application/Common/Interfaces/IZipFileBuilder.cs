using CapitalRaising.RightsIssues.Service.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface IZipFileBuilder
    {
        Task<byte[]> BuildFileAsync(IEnumerable<ZipFileEntry> zipFileEntries);
    }
}
