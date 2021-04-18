using MyHealthSolution.Service.Domain.CustomEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyHealthSolution.Service.Infrastructure.Persistence.Configurations
{
    public class OfferPaymentRecordConfiguration : IEntityTypeConfiguration<OfferPaymentRecord>
    {
        public void Configure(EntityTypeBuilder<OfferPaymentRecord> builder)
        {
            builder.HasNoKey();
        }
    }
}
