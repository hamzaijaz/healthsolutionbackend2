using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyHealthSolution.Service.Application.Common.Exceptions;
using MyHealthSolution.Service.Application.Common.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;

namespace MyHealthSolution.Service.Infrastructure.Files
{
    public class CsvFileReader : ICsvFileReader
    {
        private IEnumerable<ClassMap> _classMaps;

        public CsvFileReader(IEnumerable<ClassMap> classMaps)
        {
            _classMaps = classMaps;
        }

        // NOTE: We are not returning AsyncEnumerable here due to problems with
        //  1. deferred execution
        //  2. resources which are disposed after the method finishes execution
        // 
        // Deferred calls to this method will result in errors due to objects being disposed
        public async Task<IEnumerable<TRecord>> ReadAsync<TRecord>(Stream stream)
        {
            var isRecordBad = false;
            var badRecords = new List<string>();

            using(var streamReader = new StreamReader(stream))
            {
                if (streamReader.EndOfStream)
                {
                    throw new BadRequestException("File contents not provided");
                }
                using(var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    // register all class maps
                    foreach(CsvHelper.Configuration.ClassMap c in _classMaps)
                    {
                        csvReader.Configuration.RegisterClassMap(c);
                        csvReader.Configuration.BadDataFound = (ctx) => 
                        {
                            isRecordBad = true;
                            badRecords.Add("Row " + ctx.Row + " -> " + ctx.RawRecord);
                        }; 
                    }

                    var records = new List<TRecord>();
                    try
                    {
                        await foreach(var record in csvReader.GetRecordsAsync<TRecord>())
                        {
                            if(!isRecordBad)
                                records.Add(record);

                            isRecordBad=false;
                        }    
                    }
                    catch (CsvHelperException ex)
                    {
                        throw new BadRequestException($"Bad record found at row {ex.ReadingContext.Row}, position {ex.ReadingContext.CurrentIndex}.");
                    }

                    if(badRecords.Any())
                    {
                        // TODO add line numbers from file
                        // raise exception
                        throw new BadRequestException($"{badRecords.Count()} Bad records found. Please check the CSV file.");
                    }

                    return records;
                }
            }
        }
    }
}