using Bookify.Domain.Enums;

namespace Bookify.Infrastructure.Persistence.Configurations
{
    internal class RentalCopyConfiguration : IEntityTypeConfiguration<RentalCopy>
    {
        public void Configure(EntityTypeBuilder<RentalCopy> builder)
        {
            builder.HasKey(e => new { e.BookCopyId, e.RentalId });
            builder.HasQueryFilter(e => !e.Rental!.IsDeleted);

            builder.Property(e => e.ReturnDate).HasDefaultValueSql("CAST(GETDATE() AS DATE)");
        }
    }
}
