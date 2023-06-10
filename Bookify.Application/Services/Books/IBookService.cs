namespace Bookify.Application.Services;
public interface IBookService
{
    Book? GetById(int id);
    Book? GetWithCategories(int id);
    IEnumerable<BookDto> GetLastAddedBooks(int numberOfBooks);
    IEnumerable<BookDto> GetTopBooks(int numberOfBooks);
    PaginatedList<Book> GetPaginatedList(IList<int> selectedAuthors, IList<int> selectedCategories, int pageNumber, int pageSize);
    IQueryable<Book> GetQuerbaleRawData(string authors, string categories);
    IQueryable<Book> GetDetails();
    IQueryable<Book> Search(string query);
    (IQueryable<Book> books, int count) GetFiltered(GetFilteredDto dto);
    Book Add(Book book, IEnumerable<int> selectedCategories, string createdById);
    Book Update(Book book, IEnumerable<int> selectedCategories, string updatedById);
    Book? ToggleStatus(int id, string updatedById);
    bool AllowTitle(int id, string title, int authorId);
    int GetActiveBooksCount();
}
