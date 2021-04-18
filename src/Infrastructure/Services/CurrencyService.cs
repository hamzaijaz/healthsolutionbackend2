using MyHealthSolution.Service.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MyHealthSolution.Service.Infrastructure.Services
{
    public class CurrencyService: ICurrencyService
    {
		//Create and store a dictionary of <CurrencyCode, CurrencySymbol>
		//And then use it in GetSymbol method to get CurrencySymbol from a given CurrencyCode
		private readonly Dictionary<string, string> SymbolsByCode;

		public CurrencyService()
		{			
			SymbolsByCode = new Dictionary<string, string>();
			var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
								//https://andrewlock.net/dotnet-core-docker-and-cultures-solving-culture-issues-porting-a-net-core-app-from-windows-to-linux/
								.Where(culture => culture.LCID != 0x7F && culture.LCID != 4096 && culture.LCID != 0x1000) // filter invariant culture whcih causes error
								.Select(x => new RegionInfo(x.LCID));

			foreach (var region in regions)
			{
				if (!SymbolsByCode.ContainsKey(region.ISOCurrencySymbol))
				{
					SymbolsByCode.Add(region.ISOCurrencySymbol, region.CurrencySymbol);
				}
			}
		}

		public string GetSymbol(string code)
		{
			return SymbolsByCode[code];
		}
	}
}
