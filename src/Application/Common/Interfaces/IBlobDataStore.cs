using System;
using System.IO;
using System.Threading.Tasks;

namespace MyHealthSolution.Service.Application.Common.Interfaces
{
    public interface IBlobDataStore
    {
        Task<Stream> DownloadFileAsync(string folder, string fileName);
      
        Task UploadFileAsync(string folder, string fileName, Stream data);
    }


}
