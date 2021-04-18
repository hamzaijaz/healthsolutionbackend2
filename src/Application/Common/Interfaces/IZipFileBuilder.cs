using MyHealthSolution.Service.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MyHealthSolution.Service.Application.Common.Interfaces
{
    public interface IZipFileBuilder
    {
        Task<byte[]> BuildFileAsync(IEnumerable<ZipFileEntry> zipFileEntries);
    }
}
