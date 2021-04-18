using MyHealthSolution.Service.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace MyHealthSolution.Service.Infrastructure.UnitTests.Services
{
    public class CurrencyServiceTests
    {
        private readonly CurrencyService _currencyService = new CurrencyService();

        [Fact]
        public void GetSymbol_Should_Return_Correct_Response()
        {
            _currencyService.GetSymbol("EUR").Should().Be("€");
            _currencyService.GetSymbol("GBP").Should().Be("£");
        }
    }
}
