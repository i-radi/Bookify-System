namespace Bookify.Application.Services;
internal class BookCopyService : IBookCopyService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookCopyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public BookCopy? GetById(int id)
    {
        return _unitOfWork.BookCopies.GetById(id);
    }

    public BookCopy? GetActiveCopyBySerialNumber(string serialNumber)
    {
        return _unitOfWork.BookCopies
                    .Find(predicate: c => c.SerialNumber.ToString() == serialNumber && !c.IsDeleted && !c.Book!.IsDeleted,
                          include: c => c.Include(x => x.Book)!);
    }

    public BookCopy? GetDetails(int id)
    {
        return _unitOfWork.BookCopies.Find(c => c.Id == id,
                    include: c => c.Include(x => x.Book)!);
    }

    public IEnumerable<BookCopy> GetRentalCopies(IEnumerable<int> copies)
    {
        return _unitOfWork.BookCopies.FindAll(
                predicate: c => copies.Contains(c.Id),
                include: c => c.Include(x => x.Book)!
            );
    }

    public BookCopy? Add(int bookId, int editionNumber, bool isAvailableForRental, string createdById)
    {
        var book = _unitOfWork.Books.GetById(bookId);

        if (book is null)
            return null;

        BookCopy copy = new()
        {
            EditionNumber = editionNumber,
            IsAvailableForRental = book.IsAvailableForRental && isAvailableForRental,
            CreatedById = createdById
        };

        book.Copies.Add(copy);
        _unitOfWork.Complete();

        return copy;
    }

    public BookCopy? Update(int id, int editionNumber, bool isAvailableForRental, string updatedById)
    {
        var copy = GetDetails(id);

        if (copy is null)
            return null;

        copy.EditionNumber = editionNumber;
        copy.IsAvailableForRental = copy.Book!.IsAvailableForRental && isAvailableForRental;
        copy.LastUpdatedById = updatedById;
        copy.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return copy;
    }

    public BookCopy? ToggleStatus(int id, string updatedById)
    {
        var copy = GetById(id);

        if (copy is null)
            return null;

        copy.IsDeleted = !copy.IsDeleted;
        copy.LastUpdatedById = updatedById;
        copy.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return copy;
    }

    public (string errorMessage, ICollection<RentalCopy> copies) CanBeRented(IEnumerable<int> selectedSerials, int subscriberId, int? rentalId = null)
    {
        var selectedCopies = _unitOfWork.BookCopies
            .FindAll(predicate: c => selectedSerials.Contains(c.SerialNumber),
                    include: c => c.Include(c => c.Book).Include(c => c.Rentals));

        var query = _unitOfWork.Rentals.GetQueryable();

        var currentSubscriberRentals = query
            .Include(r => r.RentalCopies)
            .ThenInclude(c => c.BookCopy)
            .Where(r => r.SubscriberId == subscriberId && (rentalId == null || r.Id != rentalId))
            .SelectMany(r => r.RentalCopies)
            .Where(c => !c.ReturnDate.HasValue)
            .Select(c => c.BookCopy!.BookId)
            .ToList();

        List<RentalCopy> copies = new();

        foreach (var copy in selectedCopies)
        {
            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return (errorMessage: Errors.NotAvilableRental, copies);

            if (copy.Rentals.Any(c => !c.ReturnDate.HasValue && (rentalId == null || c.RentalId != rentalId)))
                return (errorMessage: Errors.CopyIsInRental, copies);

            if (currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                return (errorMessage: $"This subscriber already has a copy for '{copy.Book.Title}' Book", copies);

            copies.Add(new RentalCopy { BookCopyId = copy.Id });
        }

        return (errorMessage: string.Empty, copies);
    }

    public bool CopyIsInRental(int id)
    {
        return _unitOfWork.RentalCopies.IsExists(c => c.BookCopyId == id && !c.ReturnDate.HasValue);
    }
}