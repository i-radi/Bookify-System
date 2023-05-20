namespace Bookify.Infrastructure.Persistence.Configurations;
internal class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.StartDate).HasDefaultValueSql("CAST(GETDATE() AS Date)");
        builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
    }
}