namespace Bookify.Infrastructure.Persistence.Configurations
{
    internal class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.HasQueryFilter(r => !r.IsDeleted);

            builder.Property(e => e.StartDate).HasDefaultValueSql("CAST(GETDATE() AS DATE)");
            builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
        }
    }
}
