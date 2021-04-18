using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Converters;

namespace MyHealthSolution.Service.FunctionApp.JsonConverters
{
    [ExcludeFromCodeCoverage]
    public class ShortDateConverter : IsoDateTimeConverter
    {
        public ShortDateConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
