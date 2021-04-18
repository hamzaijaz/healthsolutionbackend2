using Application.IntegrationTests;
using MyHealthSolution.Service.Application.Common.Exceptions;
using MyHealthSolution.Service.Application.Patients.Commands.CreatePatient;
using MyHealthSolution.Service.Application.Patients.Queries;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MyHealthSolution.Service.Application.IntegrationTests.Patients.Queries
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

        [Fact]
        public async Task ShouldReturnPatient()
        {
            var command = CreatePatientCommand();

            // Send command
            var result = await Testing.SendAsync(command, this.Output);

            result.Should().NotBeNull();
            result.Patient.Should().NotBeNull();
            result.Patient.FirstName.Should().Be("Mike");

            var patientKey = result.Patient.PatientKey;

            // Use a query to retrieve the result to confirm it was stored
            var query = new GetPatientQuery();
            query.PatientKey = patientKey;

            var queryResult = await Testing.SendAsync(query, this.Output);

            queryResult.Should().NotBeNull();
            queryResult.Patient.FirstName.Should().Be("Mike");
            queryResult.Patient.LastName.Should().Be("Hussey");
            queryResult.Patient.DateOfBirth.Should().Be("13-04-1995");
            queryResult.Patient.Gender.Should().Be("Male");
            queryResult.Patient.StreetAddress.Should().Be("33 Punt Road");
            queryResult.Patient.Suburb.Should().Be("Richmond");
            queryResult.Patient.Postcode.Should().Be("3000");
            queryResult.Patient.PolicyNumber.Should().Be("123456");
            queryResult.Patient.HealthCoverType.Should().Be("Bupa");
            queryResult.Patient.PatientKey.Should().Be(patientKey);
        }

        private CreatePatientCommand CreatePatientCommand()
        {
            return new CreatePatientCommand
            {
                FirstName = "Mike",
                LastName = "Hussey",
                DateOfBirth = "13-04-1995",
                Gender = "Male",
                Postcode = "3000",
                PolicyNumber = "123456",
                Suburb = "Richmond",
                StreetAddress = "33 Punt Road",
                HealthCoverType = "Bupa"
            };
        }
    }
}
