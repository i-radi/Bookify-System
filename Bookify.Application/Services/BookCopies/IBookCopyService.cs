namespace Bookify.Application.Services;
public interface IBookCopyService
{
    BookCopy? GetById(int id);
    BookCopy? GetActiveCopyBySerialNumber(string serialNumber);
    BookCopy? GetDetails(int id);
    IEnumerable<BookCopy> GetRentalCopies(IEnumerable<int> copies);
    BookCopy? Add(int bookId, int editionNumber, bool isAvailableForRental, string createdById);
    BookCopy? Update(int id, int editionNumber, bool isAvailableForRental, string updatedById);
    BookCopy? ToggleStatus(int id, string updatedById);
    (string errorMessage, ICollection<RentalCopy> copies) CanBeRented(IEnumerable<int> selectedSerials, int subscriberId, int? rentalId = null);
    bool CopyIsInRental(int id);
}