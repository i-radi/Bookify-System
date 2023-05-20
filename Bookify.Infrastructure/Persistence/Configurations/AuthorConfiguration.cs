namespace Bookify.Infrastructure.Persistence.Configurations;
internal class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.Name).HasMaxLength(100);
        builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
    }
}