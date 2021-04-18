using Application.IntegrationTests;
using CapitalRaising.RightsIssues.Service.Application.Patients.Commands.CreatePatient;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using CapitalRaising.RightsIssues.Service.Application.Common.Exceptions;
using System.Threading.Tasks;
using CapitalRaising.RightsIssues.Service.Application.Patients.Queries;
using Entities = CapitalRaising.RightsIssues.Service.Domain.Entities;

namespace CapitalRaising.RightsIssues.Service.Application.IntegrationTests.Patients.Commands
{
    [Collection("Tests")] // XUnit Requirement
    public class CreatePatientTests : TestBase
    {
        public CreatePatientTests(ITestOutputHelper output) : base(output)
        {
        }

        #region Validations

        [Fact]
        public void ShouldRequireMinimumFields()
        {
            var command = new CreatePatientCommand();

            FluentActions.Invoking(() =>
                Testing.SendAsync(command, this.Output)).Should().Throw<ValidationException>();
        }

        [Fact]
        public void ShouldRequireFirstName()
        {
            // arrange
            var command = CreatePatientCommand();
            command.FirstName = "";

            // Act ,Assert
            FluentActions.Invoking(() =>
                    Testing.SendAsync(command, this.Output))
                .Should()
                .Throw<ValidationException>();
        }
        #endregion

        [Fact]
        public async Task ShouldCreatePatient()
        {
            var patientKey = Guid.NewGuid();
            var command = CreatePatientCommand(patientKey);

            // Send command
            var result = await Testing.SendAsync(command, this.Output);

            result.Should().NotBeNull();
            result.Patient.Should().NotBeNull();
            result.Patient.FirstName.Should().Be("Mike");
            // Use a query to retrieve the result to confirm it was stored
            var query = new GetPatientQuery();
            query.PatientKey = patientKey;

            var queryResult = await Testing.SendAsync(query, this.Output);

            queryResult.Should().NotBeNull();
            queryResult.Patient.FirstName.Should().Be("Mike");
            queryResult.Patient.LastName.Should().Be("Hussey");

        }

        private CreatePatientCommand CreatePatientCommand(Guid? patientKey = null)
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