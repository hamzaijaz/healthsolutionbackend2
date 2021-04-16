using AutoMapper;
using CapitalRaising.RightsIssues.Service.Application.Common.Mappings;

namespace CapitalRaising.RightsIssues.Service.Application.UnitTests.Common.Mappings
{
    public class MappingTestsFixture
    {
        public MappingTestsFixture()
        {
            ConfigurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Application.Common.Mappings.MappingProfile>();
                cfg.AddProfile<FunctionApp.Mappings.MappingProfile>();
            });

            Mapper = ConfigurationProvider.CreateMapper();
        }

        public IConfigurationProvider ConfigurationProvider { get; }

        public IMapper Mapper { get; }
    }
}
