namespace Bookify.Application.Services;
internal class SubscriberService : ISubscriberService
{
    private readonly IUnitOfWork _unitOfWork;

    public SubscriberService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Subscriber? GetById(int id)
    {
        return _unitOfWork.Subscribers.GetById(id);
    }

    public IQueryable<Subscriber> GetQueryable()
    {
        return _unitOfWork.Subscribers.GetQueryable();
    }

    public IQueryable<Subscriber> GetQueryableDetails()
    {
        return _unitOfWork.Subscribers.GetQueryable()
                .Include(s => s.Governorate)
                .Include(s => s.Area)
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies);
    }

    public Subscriber? GetSubscriberWithRentals(int id)
    {
        return _unitOfWork.Subscribers.GetQueryable()
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == id);
    }

    public Subscriber? GetSubscriberWithSubscriptions(int id)
    {
        return _unitOfWork.Subscribers.GetQueryable()
                .Include(s => s.Subscriptions)
                .SingleOrDefault(s => s.Id == id);
    }

    public int GetActiveSubscribersCount()
    {
        return _unitOfWork.Subscribers.Count(c => !c.IsDeleted);
    }

    public IEnumerable<KeyValuePairDto> GetSubscribersPerCity()
    {
        return _unitOfWork.Subscribers.GetQueryable()
            .Include(s => s.Governorate)
                .Where(s => !s.IsDeleted)
                .GroupBy(s => new { GovernorateName = s.Governorate!.Name })
                .Select(g => new KeyValuePairDto(
                    g.Key.GovernorateName,
                    g.Count().ToString()
                ))
                .ToList();
    }

    public IEnumerable<Subscriber> GetExpired(int expiredWithin)
    {
        return _unitOfWork.Subscribers.GetQueryable()
                .Include(s => s.Subscriptions)
                .Where(s => !s.IsBlackListed && s.Subscriptions.OrderByDescending(x => x.EndDate).First().EndDate == DateTime.Today.AddDays(expiredWithin))
                .ToList();
    }

    public (string errorMessage, int? maxAllowedCopies) CanRent(int id, int? rentalId = null)
    {
        var subscriber = GetSubscriberWithRentals(id);

        if (subscriber is null)
            return (errorMessage: Errors.NotFoundSubscriber, maxAllowedCopies: null);

        if (subscriber.IsBlackListed)
            return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

        if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration))
            return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);

        var currentRentals = subscriber.Rentals
            .Where(r => rentalId == null || r.Id != rentalId)
            .SelectMany(r => r.RentalCopies)
            .Count(c => !c.ReturnDate.HasValue);

        var availableCopiesCount = (int)RentalsConfigurations.MaxAllowedCopies - currentRentals;

        if (availableCopiesCount.Equals(0))
            return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

        return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
    }

    public Subscriber Add(Subscriber subscriber, string imagePath, string imageName, string createdById)
    {
        subscriber.ImageUrl = $"{imagePath}/{imageName}";
        subscriber.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";
        subscriber.CreatedById = createdById;

        Subscription subscription = new()
        {
            CreatedById = subscriber.CreatedById,
            CreatedOn = subscriber.CreatedOn,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1)
        };

        subscriber.Subscriptions.Add(subscription);

        _unitOfWork.Subscribers.Add(subscriber);
        _unitOfWork.Complete();

        return subscriber;
    }

    public void Update(Subscriber subscriber, string updatedById)
    {
        subscriber.LastUpdatedById = updatedById;
        subscriber.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();
    }

    public Subscription RenewSubscription(int id, DateTime startDate, string createdById)
    {
        Subscription subscription = new()
        {
            SubscriberId = id,
            CreatedById = createdById,
            CreatedOn = DateTime.Now,
            StartDate = startDate,
            EndDate = startDate.AddYears(1)
        };

        _unitOfWork.Subscriptions.Add(subscription);
        _unitOfWork.Complete();

        return subscription;
    }

    public bool AllowNationalId(int id, string nationalId)
    {
        var subscriber = _unitOfWork.Subscribers.Find(c => c.NationalId == nationalId);
        return subscriber is null || subscriber.Id.Equals(id);
    }

    public bool AllowMobileNumber(int id, string mobileNumber)
    {
        var subscriber = _unitOfWork.Subscribers.Find(c => c.MobileNumber == mobileNumber);
        return subscriber is null || subscriber.Id.Equals(id);
    }

    public bool AllowEmail(int id, string email)
    {
        var subscriber = _unitOfWork.Subscribers.Find(c => c.Email == email);
        return subscriber is null || subscriber.Id.Equals(id);
    }
}