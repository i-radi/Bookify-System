namespace Bookify.Application.Common.Interfaces;
public interface IApplicationDbContext
{
    public DbSet<Area> Areas { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookCategory> BookCategories { get; set; }
    public DbSet<BookCopy> BookCopies { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Governorate> Governorates { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<RentalCopy> RentalCopies { get; set; }
    public DbSet<Subscriber> Subscribers { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }

    int SaveChanges();
}