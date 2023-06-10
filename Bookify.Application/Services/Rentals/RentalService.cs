namespace Bookify.Application.Services;
internal class RentalService : IRentalService
{
    private readonly IUnitOfWork _unitOfWork;

    public RentalService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Rental? GetDetails(int id)
    {
        return _unitOfWork.Rentals.GetQueryable()
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .SingleOrDefault(r => r.Id == id);
    }

    public IQueryable<Rental?> GetQueryableDetails(int id)
    {
        return _unitOfWork.Rentals.GetQueryable()
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .ThenInclude(c => c!.Book);
    }

    public IEnumerable<RentalCopy> GetAllByCopyId(int copyId)
    {
        return _unitOfWork.RentalCopies
                .FindAll(predicate: c => c.BookCopyId == copyId,
                    include: c => c.Include(x => x.Rental)!.ThenInclude(x => x!.Subscriber)!,
                    orderBy: c => c.RentalDate,
                    orderByDirection: OrderBy.Descending);
    }

    public IEnumerable<KeyValuePairDto> GetRentalsPerDay(DateTime? startDate, DateTime? endDate)
    {
        return _unitOfWork.RentalCopies.GetQueryable()
                .Where(c => c.RentalDate >= startDate && c.RentalDate <= endDate)
                .GroupBy(c => new { Date = c.RentalDate })
                .Select(g => new KeyValuePairDto(
                    g.Key.Date.ToString("d MMM"),
                    g.Count().ToString()
                ))
                .ToList();
    }

    public int GetNumberOfCopies(int id)
    {
        return _unitOfWork.RentalCopies.Count(c => c.RentalId == id);
    }

    public IEnumerable<Rental> GetExpired(DateTime expiredOn)
    {
        return _unitOfWork.Rentals.GetQueryable()
                .Include(r => r.Subscriber)
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .ThenInclude(bc => bc!.Book)
                .Where(r => r.RentalCopies.Any(r => r.EndDate.Date == expiredOn && !r.ReturnDate.HasValue))
                .ToList();
    }

    public Rental Add(int subscriberId, ICollection<RentalCopy> copies, string createdById)
    {
        var rental = new Rental()
        {
            SubscriberId = subscriberId,
            RentalCopies = copies,
            CreatedById = createdById
        };

        _unitOfWork.Rentals.Add(rental);
        _unitOfWork.Complete();

        return rental;
    }

    public Rental Update(int id, ICollection<RentalCopy> copies, string updatedById)
    {
        var rental = _unitOfWork.Rentals.GetById(id);

        rental!.RentalCopies = copies;
        rental.LastUpdatedById = updatedById;
        rental.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return rental;
    }

    public void Return(Rental rental, IList<ReturnCopyDto> copies, bool penaltyPaid, string updatedById)
    {
        var isUpdated = false;

        foreach (var copy in copies)
        {
            if (!copy.IsReturned.HasValue) continue;

            var currentCopy = rental.RentalCopies.SingleOrDefault(c => c.BookCopyId == copy.Id);

            if (currentCopy is null) continue;

            if (copy.IsReturned.HasValue && copy.IsReturned.Value)
            {
                if (currentCopy.ReturnDate.HasValue) continue;

                currentCopy.ReturnDate = DateTime.Now;
                isUpdated = true;
            }

            if (copy.IsReturned.HasValue && !copy.IsReturned.Value)
            {
                if (currentCopy.ExtendedOn.HasValue) continue;

                currentCopy.ExtendedOn = DateTime.Now;
                currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)RentalsConfigurations.MaxRentalDuration);
                isUpdated = true;
            }
        }

        if (isUpdated)
        {
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = updatedById;
            rental.PenaltyPaid = penaltyPaid;

            _unitOfWork.Complete();
        }
    }

    public bool AllowExtend(Rental rental, Subscriber subscriber)
    {
        return !subscriber.IsBlackListed
                    && subscriber!.Subscriptions.Last().EndDate >= rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration)
                    && rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) >= DateTime.Today;
    }

    public string? ValidateExtendedCopies(Rental rental, Subscriber subscriber)
    {
        string error = string.Empty;

        if (subscriber!.IsBlackListed)
            error = Errors.RentalNotAllowedForBlacklisted;

        else if (subscriber!.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration))
            error = Errors.RentalNotAllowedForInactive;

        else if (rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) < DateTime.Today)
            error = Errors.ExtendNotAllowed;

        return error;
    }

    public Rental? MarkAsDeleted(int id, string deletedById)
    {
        var rental = _unitOfWork.Rentals.GetById(id);

        if (rental is null || rental.CreatedOn.Date != DateTime.Today)
            return null;

        rental.IsDeleted = true;
        rental.LastUpdatedOn = DateTime.Now;
        rental.LastUpdatedById = deletedById;

        _unitOfWork.Complete();

        return rental;
    }

    public PaginatedList<RentalCopy> GetPaginatedList(DateTime from, DateTime to, int pageNumber, int pageSize)
    {
        IQueryable<RentalCopy> rentals = _unitOfWork.RentalCopies.GetQueryable()
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .ThenInclude(b => b!.Author)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(r => r.RentalDate >= from && r.RentalDate <= to);

        return PaginatedList<RentalCopy>.Create(rentals, pageNumber, pageSize);
    }

    public IQueryable<RentalCopy> GetQuerbaleRawData(string duration)
    {
        var from = DateTime.Parse(duration.Split(" - ")[0]);
        var to = DateTime.Parse(duration.Split(" - ")[1]);

        return _unitOfWork.RentalCopies.GetQueryable()
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .ThenInclude(b => b!.Author)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(r => !string.IsNullOrEmpty(duration) && r.RentalDate >= from && r.RentalDate <= to);
    }

    public IQueryable<RentalCopy> GetQuerbaleDelayedRawData()
    {
        return _unitOfWork.RentalCopies.GetQueryable()
                           .Include(c => c.BookCopy)
                           .ThenInclude(r => r!.Book)
                           .Include(c => c.Rental)
                           .ThenInclude(c => c!.Subscriber)
                           .Where(c => !c.ReturnDate.HasValue && c.EndDate < DateTime.Today);
    }
}
