namespace Bookify.Infrastructure.Persistence.Configurations;
internal class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasIndex(e => new { e.Title, e.AuthorId }).IsUnique();

        builder.Property(e => e.Title).HasMaxLength(500);
        builder.Property(e => e.Publisher).HasMaxLength(200);
        builder.Property(e => e.Hall).HasMaxLength(50);
        builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
    }
}