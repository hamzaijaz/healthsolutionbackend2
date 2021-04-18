using System.IO;

namespace MyHealthSolution.Service.Application.IntegrationTests.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ToArray(this Stream inputStream)
        {
            using(var tempStream = new MemoryStream())
            {
                inputStream.CopyTo(tempStream);
                return tempStream.ToArray();
            }
        }
    }
}
