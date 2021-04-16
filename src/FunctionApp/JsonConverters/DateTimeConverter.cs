using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Converters;

namespace CapitalRaising.RightsIssues.Service.FunctionApp.JsonConverters
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
