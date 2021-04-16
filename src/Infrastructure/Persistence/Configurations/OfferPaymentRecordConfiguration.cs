using CapitalRaising.RightsIssues.Service.Domain.CustomEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapitalRaising.RightsIssues.Service.Infrastructure.Persistence.Configurations
{
    public class OfferPaymentRecordConfiguration : IEntityTypeConfiguration<OfferPaymentRecord>
    {
        public void Configure(EntityTypeBuilder<OfferPaymentRecord> builder)
        {
            builder.HasNoKey();
        }
    }
}
