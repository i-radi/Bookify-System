namespace Bookify.Infrastructure.Persistence.Repositories;
internal class BookCopyRepository : BaseRepository<BookCopy>, IBookCopyRepository
{
    public BookCopyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public void SetAllAsNotAvailable(int bookId)
    {
        _context.BookCopies.Where(c => c.BookId == bookId)
                    .ExecuteUpdate(p => p.SetProperty(c => c.IsAvailableForRental, false));
    }
}