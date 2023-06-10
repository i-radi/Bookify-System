namespace Bookify.Application.Common.Interfaces.Repositories;
public interface IBookRepository : IBaseRepository<Book>
{
    IQueryable<Book> GetDetails();
}