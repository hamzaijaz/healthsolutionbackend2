using MyHealthSolution.Service.Domain.CustomEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyHealthSolution.Service.Infrastructure.Persistence.Configurations
{
    public class OfferAcceptanceRecordConfiguration : IEntityTypeConfiguration<OfferAcceptanceRecord>
    {
        public void Configure(EntityTypeBuilder<OfferAcceptanceRecord> builder)
        {
            builder.HasNoKey();
        }
    }
}
