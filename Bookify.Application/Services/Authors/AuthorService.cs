namespace Bookify.Application.Services;
internal class AuthorService : IAuthorService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Author> GetAll()
    {
        return _unitOfWork.Authors.GetAll(withNoTracking: false);
    }

    public IEnumerable<Author> GetActiveAuthors()
    {
        return _unitOfWork.Authors.FindAll(predicate: a => !a.IsDeleted, orderBy: a => a.Name, OrderBy.Ascending);
    }

    public Author? GetById(int id)
    {
        return _unitOfWork.Authors.GetById(id);
    }

    public Author Add(string name, string createdById)
    {
        var author = new Author
        {
            Name = name,
            CreatedById = createdById
        };

        _unitOfWork.Authors.Add(author);
        _unitOfWork.Complete();

        return author;
    }

    public Author? Update(int id, string name, string updatedById)
    {
        var author = GetById(id);

        if (author is null)
            return null;

        author.Name = name;
        author.LastUpdatedById = updatedById;
        author.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return author;
    }

    public Author? ToggleStatus(int id, string updatedById)
    {
        var author = GetById(id);

        if (author is null)
            return null;

        author.IsDeleted = !author.IsDeleted;
        author.LastUpdatedById = updatedById;
        author.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return author;
    }

    public bool AllowAuthor(int id, string name)
    {
        var author = _unitOfWork.Authors.Find(c => c.Name == name);
        return author is null || author.Id.Equals(id);
    }
}