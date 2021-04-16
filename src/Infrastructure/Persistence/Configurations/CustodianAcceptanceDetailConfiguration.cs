using CapitalRaising.RightsIssues.Service.Domain.CustomEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapitalRaising.RightsIssues.Service.Infrastructure.Persistence.Configurations
{
    public class CustodianAcceptanceDetailConfiguration : IEntityTypeConfiguration<CustodianAcceptanceDetail>
    {
        public void Configure(EntityTypeBuilder<CustodianAcceptanceDetail> builder)
        {
            builder.HasNoKey();
        }
    }
}
