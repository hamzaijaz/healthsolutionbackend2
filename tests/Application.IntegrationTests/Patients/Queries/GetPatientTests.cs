using Application.IntegrationTests;
using CapitalRaising.RightsIssues.Service.Application.Common.Exceptions;
using CapitalRaising.RightsIssues.Service.Application.Patients.Queries;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CapitalRaising.RightsIssues.Service.Application.IntegrationTests.Patients.Queries
{
    [Collection("Tests")] // XUnit Requirement
    public class GetPatientTests : TestBase
    {
        public GetPatientTests(ITestOutputHelper output) : base(output)
        {
        }

        #region Validations

        [Fact]
        public void ShouldRequireMinimumFields()
        {
            var query = new GetPatientQuery();

            FluentActions.Invoking(() =>
                Testing.SendAsync(query, this.Output)).Should().Throw<ValidationException>();
        }

        #endregion
    }
}
