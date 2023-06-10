namespace Bookify.Infrastructure.Persistence.Repositories;
internal class BookRepository : BaseRepository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext context) : base(context)
    {
    }

    public IQueryable<Book> GetDetails()
    {
        return _context.Books
                .Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);
    }
}