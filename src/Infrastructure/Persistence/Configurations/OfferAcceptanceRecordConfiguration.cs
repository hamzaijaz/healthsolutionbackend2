using CapitalRaising.RightsIssues.Service.Domain.CustomEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapitalRaising.RightsIssues.Service.Infrastructure.Persistence.Configurations
{
    public class OfferAcceptanceRecordConfiguration : IEntityTypeConfiguration<OfferAcceptanceRecord>
    {
        public void Configure(EntityTypeBuilder<OfferAcceptanceRecord> builder)
        {
            builder.HasNoKey();
        }
    }
}
