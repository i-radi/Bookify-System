namespace Bookify.Infrastructure.Persistence.Configurations;
internal class RentalCopyConfiguration : IEntityTypeConfiguration<RentalCopy>
{
    public void Configure(EntityTypeBuilder<RentalCopy> builder)
    {
        builder.HasKey(e => new { e.RentalId, e.BookCopyId });
        builder.HasQueryFilter(e => !e.Rental!.IsDeleted);

        builder.Property(e => e.RentalDate).HasDefaultValueSql("CAST(GETDATE() AS Date)");
    }
}