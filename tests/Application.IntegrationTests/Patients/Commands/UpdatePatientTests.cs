using Application.IntegrationTests;
using MyHealthSolution.Service.Application.Patients.Commands.CreatePatient;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using MyHealthSolution.Service.Application.Common.Exceptions;
using System.Threading.Tasks;
using MyHealthSolution.Service.Application.Patients.Queries;
using MyHealthSolution.Service.Application.Patients.Commands.UpdatePatient;
using System;

namespace MyHealthSolution.Service.Application.IntegrationTests.Patients.Commands
{
    [Collection("Tests")] // XUnit Requirement
    public class UpdatePatientTests : TestBase
    {
        public UpdatePatientTests(ITestOutputHelper output) : base(output)
        {
        }

        #region Validations

        [Fact]
        public void ShouldRequireMinimumFields()
        {
            var command = new UpdatePatientCommand();

            FluentActions.Invoking(() =>
                Testing.SendAsync(command, this.Output)).Should().Throw<ValidationException>();
        }

        [Fact]
        public void ShouldRequireFirstName()
        {
            // arrange
            var command = UpdatePatientCommand();
            command.FirstName = "";

            // Act ,Assert
            FluentActions.Invoking(() =>
                    Testing.SendAsync(command, this.Output))
                .Should()
                .Throw<ValidationException>();
        }

        [Fact]
        public void ShouldReturnNotFoundException()
        {
            // arrange
            var command = UpdatePatientCommand();
            command.PatientKey = Guid.NewGuid();
            // Act ,Assert
            FluentActions.Invoking(() =>
                    Testing.SendAsync(command, this.Output))
                .Should()
                .Throw<NotFoundException>();
        }

        //can write similar validation tests for all other fields
        #endregion

        [Fact]
        public async Task ShouldUpdatePatient()
        {
            var command = CreatePatientCommand();

            // Send command
            var result = await Testing.SendAsync(command, this.Output);

            result.Should().NotBeNull();
            result.Patient.Should().NotBeNull();
            result.Patient.FirstName.Should().Be("Mike");

            var patientKey = result.Patient.PatientKey;
            // Use a query to retrieve the result to confirm it was stored
            var query = UpdatePatientCommand();
            query.PatientKey = patientKey;

            var queryResult = await Testing.SendAsync(query, this.Output);

            queryResult.Should().NotBeNull();
            queryResult.Patient.FirstName.Should().Be("Mike1");
            queryResult.Patient.LastName.Should().Be("Hussey1");
            queryResult.Patient.PatientKey.Should().Be(patientKey);
            queryResult.Patient.Gender.Should().Be("Female");
            queryResult.Patient.HealthCoverType.Should().Be("Bupaa");
            queryResult.Patient.DateOfBirth.Should().Be("14-04-1995");
            queryResult.Patient.PolicyNumber.Should().Be("1234567");
            queryResult.Patient.StreetAddress.Should().Be("33 Punt Roadd");
            queryResult.Patient.Suburb.Should().Be("Richmondd");
            queryResult.Patient.Postcode.Should().Be("30000");
        }

        private UpdatePatientCommand UpdatePatientCommand()
        {
            return new UpdatePatientCommand
            {
                FirstName = "Mike1",
                LastName = "Hussey1",
                DateOfBirth = "14-04-1995",
                Gender = "Female",
                Postcode = "30000",
                PolicyNumber = "1234567",
                Suburb = "Richmondd",
                StreetAddress = "33 Punt Roadd",
                HealthCoverType = "Bupaa",
            };
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
                HealthCoverType = "Bupa",
                RecaptchaResponse = "someText"
            };
        }
    }
}