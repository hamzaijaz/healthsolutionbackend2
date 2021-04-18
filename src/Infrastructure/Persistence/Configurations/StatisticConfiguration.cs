using MyHealthSolution.Service.Domain.CustomEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyHealthSolution.Service.Infrastructure.Persistence.Configurations
{
    public class StatisticConfiguration : IEntityTypeConfiguration<Statistic>
    {
        public void Configure(EntityTypeBuilder<Statistic> builder)
        {
            builder.HasNoKey();
        }
    }
}
