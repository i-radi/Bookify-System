namespace Bookify.Application.Common.Interfaces.Repositories;
public interface IBookCopyRepository : IBaseRepository<BookCopy>
{
    void SetAllAsNotAvailable(int bookId);
}