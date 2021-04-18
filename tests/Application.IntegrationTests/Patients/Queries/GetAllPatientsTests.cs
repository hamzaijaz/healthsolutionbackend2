using Application.IntegrationTests;
using FluentAssertions;
using MyHealthSolution.Service.Application.Patients.Commands.CreatePatient;
using MyHealthSolution.Service.Application.Patients.Queries;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MyHealthSolution.Service.Application.IntegrationTests.Patients.Queries
{
    [Collection("Tests")] // XUnit Requirement
    public class GetAllPatientsTests : TestBase
    {
        public GetAllPatientsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ShouldReturnNoPatient()
        {
            //querying data without storing any patient in database
            var query = new GetAllPatientsQuery();

            var queryResult = await Testing.SendAsync(query, this.Output);

            queryResult.Should().NotBeNull();
            queryResult.Patients.Count.Should().Be(0);
        }

        [Fact]
        public async Task ShouldReturnCorrectPatient()
        {
            var command = CreatePatientCommand();

            // Create a new patient first
            var result = await Testing.SendAsync(command, this.Output);

            var patientKey = result.Patient.PatientKey;

            // Use a query to retrieve the result to confirm it was stored
            var query = new GetAllPatientsQuery();

            var queryResult = await Testing.SendAsync(query, this.Output);

            queryResult.Should().NotBeNull();
            queryResult.Patients.Count.Should().Be(1);

            var patient1 = queryResult.Patients.FirstOrDefault();
            patient1.FirstName.Should().Be("Mike");
            patient1.LastName.Should().Be("Hussey");
            patient1.DateOfBirth.Should().Be("13-04-1995");
            patient1.Gender.Should().Be("Male");
            patient1.StreetAddress.Should().Be("33 Punt Road");
            patient1.Suburb.Should().Be("Richmond");
            patient1.Postcode.Should().Be("3000");
            patient1.PolicyNumber.Should().Be("123456");
            patient1.HealthCoverType.Should().Be("Bupa");
            patient1.PatientKey.Should().Be(patientKey);
        }

        [Fact]
        public async Task ShouldReturn3Patients()
        {
            var command1 = CreatePatientCommand(firstName: "Patient1");
            var command2 = CreatePatientCommand(firstName: "Patient2");
            var command3 = CreatePatientCommand(firstName: "Patient3");

            // Create 3 new patients
            var result = await Testing.SendAsync(command1, this.Output);
            var result2 = await Testing.SendAsync(command2, this.Output);
            var result3 = await Testing.SendAsync(command3, this.Output);

            var patient1Key = result.Patient.PatientKey;
            var patient2Key = result2.Patient.PatientKey;
            var patient3Key = result3.Patient.PatientKey;

            var query = new GetAllPatientsQuery();
            var queryResult = await Testing.SendAsync(query, this.Output);

            queryResult.Should().NotBeNull();
            queryResult.Patients.Count.Should().Be(3);

            var patient1 = queryResult.Patients.Where(_ => _.PatientKey == patient1Key).FirstOrDefault();
            patient1.FirstName.Should().Be("Patient1");
            patient1.LastName.Should().Be("Hussey");
            patient1.DateOfBirth.Should().Be("13-04-1995");
            patient1.Gender.Should().Be("Male");
            patient1.StreetAddress.Should().Be("33 Punt Road");
            patient1.Suburb.Should().Be("Richmond");
            patient1.Postcode.Should().Be("3000");
            patient1.PolicyNumber.Should().Be("123456");
            patient1.HealthCoverType.Should().Be("Bupa");
            patient1.PatientKey.Should().Be(patient1Key);

            var patient2 = queryResult.Patients.Where(_ => _.PatientKey == patient2Key).FirstOrDefault();
            patient2.FirstName.Should().Be("Patient2");
            patient2.LastName.Should().Be("Hussey");
            patient2.DateOfBirth.Should().Be("13-04-1995");
            patient2.Gender.Should().Be("Male");
            patient2.StreetAddress.Should().Be("33 Punt Road");
            patient2.Suburb.Should().Be("Richmond");
            patient2.Postcode.Should().Be("3000");
            patient2.PolicyNumber.Should().Be("123456");
            patient2.HealthCoverType.Should().Be("Bupa");
            patient2.PatientKey.Should().Be(patient2Key);

            var patient3 = queryResult.Patients.Where(_ => _.PatientKey == patient3Key).FirstOrDefault();
            patient3.FirstName.Should().Be("Patient3");
            patient3.LastName.Should().Be("Hussey");
            patient3.DateOfBirth.Should().Be("13-04-1995");
            patient3.Gender.Should().Be("Male");
            patient3.StreetAddress.Should().Be("33 Punt Road");
            patient3.Suburb.Should().Be("Richmond");
            patient3.Postcode.Should().Be("3000");
            patient3.PolicyNumber.Should().Be("123456");
            patient3.HealthCoverType.Should().Be("Bupa");
            patient3.PatientKey.Should().Be(patient3Key);
        }

        private CreatePatientCommand CreatePatientCommand(string firstName = "Mike")
        {
            return new CreatePatientCommand
            {
                FirstName = firstName,
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
