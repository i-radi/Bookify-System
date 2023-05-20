namespace Bookify.Infrastructure.Persistence.Configurations;
internal class BookCategoryConfiguration : IEntityTypeConfiguration<BookCategory>
{
    public void Configure(EntityTypeBuilder<BookCategory> builder)
    {
        builder.HasKey(e => new { e.BookId, e.CategoryId });
    }
}