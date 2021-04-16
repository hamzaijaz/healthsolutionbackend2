using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using CapitalRaising.RightsIssues.Service.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace CapitalRaising.RightsIssues.Service.Application.IntegrationTests
{
    public class InMemoryBlobDataStore : IBlobDataStore
    {
        private readonly BlobStoreConfig storeConfiguration;
        public InMemoryBlobDataStore(IOptionsMonitor<BlobStoreConfig> options)
        {
            this.storeConfiguration = options.CurrentValue;
        }
        private static ConcurrentDictionary<string, Stream> messages = new ConcurrentDictionary<string, Stream>();

        public ConcurrentDictionary<string, Stream> Messages { get => messages; }

        public Task<Stream> DownloadFileAsync(string folder, string fileName)
        {
            var key = folder + fileName;
            
            bool found = Messages.TryGetValue(key.ToLower(), out Stream data);
            if(!found && data == null )
            {
                // Specified file does not exist in blob storage.
                throw new FileNotFoundException(fileName);
            }

            data.Position = 0;
            return Task.FromResult(data);
        }

        public Task UploadFileAsync(string folder, string fileName, Stream data)
        {
            if (string.IsNullOrEmpty(folder))
            {
                throw new System.ArgumentException($"'{nameof(folder)}' cannot be null or empty", nameof(folder));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new System.ArgumentException($"'{nameof(fileName)}' cannot be null or empty", nameof(fileName));
            }

            if (data is null)
            {
                throw new System.ArgumentNullException(nameof(data));
            }

            // Stream position rewind before copying to another stream to overcome closed stream issue.
            data.Position = 0;
            MemoryStream destination = new MemoryStream();
            data.CopyTo(destination);

            var key = folder + fileName.ToLower();

            Messages.AddOrUpdate(key,
                                 destination,
                                (key, value) =>
                                {
                                    return destination;
                                });
            return Task.CompletedTask;
        }

    }
}
