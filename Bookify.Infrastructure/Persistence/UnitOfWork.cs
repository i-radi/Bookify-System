using Bookify.Infrastructure.Persistence.Repositories;

namespace Bookify.Infrastructure.Persistence;
internal class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IBaseRepository<Area> Areas => new BaseRepository<Area>(_context);
    public IBaseRepository<Author> Authors => new BaseRepository<Author>(_context);
    public IBookRepository Books => new BookRepository(_context);
    public IBaseRepository<BookCategory> BookCategories => new BaseRepository<BookCategory>(_context);
    public IBookCopyRepository BookCopies => new BookCopyRepository(_context);
    public ICategoryRepository Categories => new CategoryRepository(_context);
    public IBaseRepository<Governorate> Governorates => new BaseRepository<Governorate>(_context);
    public IBaseRepository<Rental> Rentals => new BaseRepository<Rental>(_context);
    public IBaseRepository<RentalCopy> RentalCopies => new BaseRepository<RentalCopy>(_context);
    public IBaseRepository<Subscriber> Subscribers => new BaseRepository<Subscriber>(_context);
    public IBaseRepository<Subscription> Subscriptions => new BaseRepository<Subscription>(_context);

    public int Complete()
    {
        return _context.SaveChanges();
    }
}
