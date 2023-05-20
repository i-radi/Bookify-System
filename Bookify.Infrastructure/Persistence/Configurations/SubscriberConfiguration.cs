namespace Bookify.Infrastructure.Persistence.Configurations;
internal class SubscriberConfiguration : IEntityTypeConfiguration<Subscriber>
{
    public void Configure(EntityTypeBuilder<Subscriber> builder)
    {
        builder.HasIndex(e => e.NationalId).IsUnique();
        builder.HasIndex(e => e.MobileNumber).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();

        builder.Property(e => e.FirstName).HasMaxLength(100);
        builder.Property(e => e.LastName).HasMaxLength(100);
        builder.Property(e => e.NationalId).HasMaxLength(20);
        builder.Property(e => e.MobileNumber).HasMaxLength(15);
        builder.Property(e => e.Email).HasMaxLength(150);
        builder.Property(e => e.ImageUrl).HasMaxLength(500);
        builder.Property(e => e.ImageThumbnailUrl).HasMaxLength(500);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
    }
}