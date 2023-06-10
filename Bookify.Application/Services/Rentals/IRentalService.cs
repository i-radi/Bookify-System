namespace Bookify.Application.Services;
public interface IRentalService
{
    Rental? GetDetails(int id);
    IQueryable<Rental?> GetQueryableDetails(int id);
    IEnumerable<RentalCopy> GetAllByCopyId(int copyId);
    IEnumerable<KeyValuePairDto> GetRentalsPerDay(DateTime? startDate, DateTime? endDate);
    int GetNumberOfCopies(int id);
    IEnumerable<Rental> GetExpired(DateTime expiredOn);
    Rental Add(int subscriberId, ICollection<RentalCopy> copies, string createdById);
    Rental Update(int id, ICollection<RentalCopy> copies, string updatedById);
    void Return(Rental rental, IList<ReturnCopyDto> copies, bool penaltyPaid, string updatedById);
    bool AllowExtend(Rental rental, Subscriber subscriber);
    string? ValidateExtendedCopies(Rental rental, Subscriber subscriber);
    Rental? MarkAsDeleted(int id, string deletedById);
    PaginatedList<RentalCopy> GetPaginatedList(DateTime from, DateTime to, int pageNumber, int pageSize);
    IQueryable<RentalCopy> GetQuerbaleRawData(string duration);
    IQueryable<RentalCopy> GetQuerbaleDelayedRawData();
}