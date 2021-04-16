using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Extensions
{
    /// <summary>
    /// BlobStorageDataExtensions
    /// </summary>
    public static class BlobStorageDataExtensions
    {
        /// <summary>
        /// Extension method to upload data of specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="store"></param>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async static Task UploadJsonDataAsync<T>(this IBlobDataStore store, string folder, string fileName, T data)
        {
            var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            using (MemoryStream ms = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(ms))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                jsonSerializer.Serialize(writer, data);
                await writer.FlushAsync();
                await store.UploadFileAsync(folder, fileName, ms);
            }

        }
    }
}
