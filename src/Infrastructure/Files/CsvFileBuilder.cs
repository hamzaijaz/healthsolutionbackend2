using System.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;
using MyHealthSolution.Service.Application.Common.Interfaces;

namespace MyHealthSolution.Service.Infrastructure.Files
{
    public class CsvFileBuilder : ICsvFileBuilder
    {
        private IEnumerable<ClassMap> _classMaps;

        public CsvFileBuilder(IEnumerable<ClassMap> classMaps)
        {
            _classMaps = classMaps;
        }

        public async Task<byte[]> BuildFileAsync<TRecord>(System.Collections.Generic.IEnumerable<TRecord> records)
        {
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

                // register all class maps
                foreach(CsvHelper.Configuration.ClassMap c in _classMaps)
                {
                    csvWriter.Configuration.RegisterClassMap(c);    
                }
                
                await csvWriter.WriteRecordsAsync(records);
            }

            return memoryStream.ToArray();
        }
    }
}