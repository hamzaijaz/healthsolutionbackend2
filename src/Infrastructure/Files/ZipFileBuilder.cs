using MyHealthSolution.Service.Application.Common.Interfaces;
using MyHealthSolution.Service.Application.Common.Models;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace MyHealthSolution.Service.Infrastructure.Files
{
    public class ZipFileBuilder : IZipFileBuilder
    {
        public async Task<byte[]> BuildFileAsync(IEnumerable<ZipFileEntry> zipFileEntries)
        {
            var zipMemStream = new MemoryStream();
            using (var archive = new ZipArchive(zipMemStream, ZipArchiveMode.Create, true))
            {
                foreach(var fileEntry in zipFileEntries)
                {
                    var newZipEntry = archive.CreateEntry(fileEntry.FileName);
                    using (var entryStream = newZipEntry.Open())
                    {
                        await entryStream.WriteAsync(fileEntry.FileContent);
                    }
                }
            }
            return zipMemStream.ToArray();
        }
    }
}
