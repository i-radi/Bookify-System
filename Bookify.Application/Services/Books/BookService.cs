using System.Linq.Dynamic.Core;

namespace Bookify.Application.Services;
internal class BookService : IBookService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Book? GetById(int id)
    {
        return _unitOfWork.Books.GetById(id);
    }

    public Book? GetWithCategories(int id)
    {
        return _unitOfWork.Books.Find(predicate: b => b.Id == id, include: b => b.Include(x => x.Categories));
    }

    public IEnumerable<BookDto> GetLastAddedBooks(int numberOfBooks)
    {
        var query = _unitOfWork.Books.GetQueryable();

        return query.Include(b => b.Author)
                    .Where(b => !b.IsDeleted)
                    .OrderByDescending(b => b.Id)
                    .Take(numberOfBooks)
                    .Select(b => new BookDto(
                        b.Id,
                        b.Title,
                        b.ImageThumbnailUrl,
                        b.Author!.Name
                    ))
                    .ToList();
    }

    public IEnumerable<BookDto> GetTopBooks(int numberOfBooks)
    {
        var query = _unitOfWork.RentalCopies.GetQueryable();

        return query.Include(c => c.BookCopy)
                .ThenInclude(c => c!.Book)
                .ThenInclude(b => b!.Author)
                .GroupBy(c => new
                {
                    c.BookCopy!.BookId,
                    c.BookCopy!.Book!.Title,
                    c.BookCopy!.Book!.ImageThumbnailUrl,
                    AuthorName = c.BookCopy!.Book!.Author!.Name
                })
                .Select(b => new
                {
                    b.Key.BookId,
                    b.Key.Title,
                    b.Key.ImageThumbnailUrl,
                    b.Key.AuthorName,
                    Count = b.Count()
                })
                .OrderByDescending(b => b.Count)
                .Take(numberOfBooks)
                .Select(b => new BookDto(
                    b.BookId,
                    b.Title,
                    b.ImageThumbnailUrl,
                    b.AuthorName
                ))
                .ToList();
    }

    public PaginatedList<Book> GetPaginatedList(IList<int> selectedAuthors, IList<int> selectedCategories, int pageNumber, int pageSize)
    {
        IQueryable<Book> books = _unitOfWork.Books.GetQueryable()
                        .Include(b => b.Author)
                        .Include(b => b.Categories)
                        .ThenInclude(c => c.Category)
                        .Where(b => (!selectedAuthors.Any() || selectedAuthors.Contains(b.AuthorId))
                        && (!selectedCategories.Any() || b.Categories.Any(c => selectedCategories.Contains(c.CategoryId))));

        return PaginatedList<Book>.Create(books, pageNumber, pageSize);
    }

    public IQueryable<Book> GetQuerbaleRawData(string authors, string categories)
    {
        var selectedAuthors = authors?.Split(',');
        var selectedCategories = categories?.Split(',');

        return _unitOfWork.Books.GetQueryable()
                        .Include(b => b.Author)
                        .Include(b => b.Categories)
                        .ThenInclude(c => c.Category)
                        .Where(b => (string.IsNullOrEmpty(authors) || selectedAuthors!.Contains(b.AuthorId.ToString()))
                            && (string.IsNullOrEmpty(categories) || b.Categories.Any(c => selectedCategories!.Contains(c.CategoryId.ToString()))));
    }

    public IQueryable<Book> GetDetails()
    {
        return _unitOfWork.Books.GetDetails();
    }

    public IQueryable<Book> Search(string query)
    {
        return _unitOfWork.Books.GetQueryable()
            .Include(b => b.Author)
            .Where(b => !b.IsDeleted && (b.Title.Contains(query) || b.Author!.Name.Contains(query)));
    }

    public (IQueryable<Book> books, int count) GetFiltered(GetFilteredDto dto)
    {
        IQueryable<Book> books = _unitOfWork.Books.GetDetails();

        if (!string.IsNullOrEmpty(dto.SearchValue))
            books = books.Where(b => b.Title.Contains(dto.SearchValue!) || b.Author!.Name.Contains(dto.SearchValue!));

        books = books
            .OrderBy($"{dto.SortColumn} {dto.SortColumnDirection}")
            .Skip(dto.Skip)
            .Take(dto.PageSize);

        var recordsTotal = _unitOfWork.Books.Count();

        return (books, recordsTotal);
    }

    public Book Add(Book book, IEnumerable<int> selectedCategories, string createdById)
    {
        book.CreatedById = createdById;

        foreach (var category in selectedCategories)
            book.Categories.Add(new BookCategory { CategoryId = category });

        _unitOfWork.Books.Add(book);
        _unitOfWork.Complete();

        return book;
    }

    public Book Update(Book book, IEnumerable<int> selectedCategories, string updatedById)
    {
        book.LastUpdatedById = updatedById;
        book.LastUpdatedOn = DateTime.Now;
        //book.ImageThumbnailUrl = GetThumbnailUrl(book.ImageUrl!);
        //book.ImagePublicId = imagePublicId;

        foreach (var category in selectedCategories)
            book.Categories.Add(new BookCategory { CategoryId = category });

        //.NET 6
        //if (!model.IsAvailableForRental)
        //    foreach (var copy in book.Copies)
        //        copy.IsAvailableForRental = false;

        _unitOfWork.Complete();

        //.NET 7
        if (!book.IsAvailableForRental)
            _unitOfWork.BookCopies.SetAllAsNotAvailable(book.Id);

        return book;
    }

    public Book? ToggleStatus(int id, string updatedById)
    {
        var book = GetById(id);

        if (book is null)
            return null;

        book.IsDeleted = !book.IsDeleted;
        book.LastUpdatedById = updatedById;
        book.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return book;
    }

    public bool AllowTitle(int id, string title, int authorId)
    {
        var book = _unitOfWork.Books.Find(b => b.Title == title && b.AuthorId == authorId);
        return book is null || book.Id.Equals(id);
    }

    public int GetActiveBooksCount()
    {
        return _unitOfWork.Books.Count(c => !c.IsDeleted);
    }
}