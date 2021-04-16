using AutoMapper;
using CapitalRaising.RightsIssues.Service.Application.Patients.Models;
//using CapitalRaising.RightsIssues.Service.Application.RightsIssues.Models;
using CapitalRaising.RightsIssues.Service.Domain.Entities;
using System;
using Xunit;

namespace CapitalRaising.RightsIssues.Service.Application.UnitTests.Common.Mappings
{
    public class MappingTests : IClassFixture<MappingTestsFixture>
    {
        private readonly IConfigurationProvider _configuration;
        private readonly IMapper _mapper;

        public MappingTests(MappingTestsFixture fixture)
        {
            _configuration = fixture.ConfigurationProvider;
            _mapper = fixture.Mapper;
        }

        [Fact]
        public void ShouldHaveValidConfiguration()
        {
            _configuration.AssertConfigurationIsValid();
        }
        
        [Theory]
        [InlineData(typeof(Patient), typeof(PatientDto))]
        public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
        {
            var instance = Activator.CreateInstance(source);

            _mapper.Map(instance, source, destination);
        }
    }
}
