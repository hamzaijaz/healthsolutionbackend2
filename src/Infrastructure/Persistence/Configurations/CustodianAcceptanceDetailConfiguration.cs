using MyHealthSolution.Service.Domain.CustomEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyHealthSolution.Service.Infrastructure.Persistence.Configurations
{
    public class CustodianAcceptanceDetailConfiguration : IEntityTypeConfiguration<CustodianAcceptanceDetail>
    {
        public void Configure(EntityTypeBuilder<CustodianAcceptanceDetail> builder)
        {
            builder.HasNoKey();
        }
    }
}
