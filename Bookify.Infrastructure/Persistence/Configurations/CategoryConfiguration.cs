namespace Bookify.Infrastructure.Persistence.Configurations;
internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
    }
}