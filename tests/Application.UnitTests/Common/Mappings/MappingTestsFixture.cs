using AutoMapper;
using MyHealthSolution.Service.Application.Common.Mappings;

namespace MyHealthSolution.Service.Application.UnitTests.Common.Mappings
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
