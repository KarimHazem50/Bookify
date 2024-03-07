namespace Bookify.Infrastructure.Persistence.Configurations
{
    internal class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.HasIndex(e => e.Name).IsUnique();

            builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.Name).HasMaxLength(100);
        }
    }
}
