using Bookify.Application.Common.Interfaces.Repositories;

namespace Bookify.Application.Common.Interfaces;
public interface IUnitOfWork
{
    IBaseRepository<Area> Areas { get; }
    IBaseRepository<Author> Authors { get; }
    IBookRepository Books { get; }
    IBaseRepository<BookCategory> BookCategories { get; }
    IBookCopyRepository BookCopies { get; }
    ICategoryRepository Categories { get; }
    IBaseRepository<Governorate> Governorates { get; }
    IBaseRepository<Rental> Rentals { get; }
    IBaseRepository<RentalCopy> RentalCopies { get; }
    IBaseRepository<Subscriber> Subscribers { get; }
    IBaseRepository<Subscription> Subscriptions { get; }

    int Complete();
}