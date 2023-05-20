namespace Bookify.Web.Core.Models
{
    public class RentalCopy
    {
        public int RentalId { get; set; }
        public Rental? Rental { get; set; }
        public int BookCopyId { get; set; }
        public BookCopy? BookCopy { get; set; }
        public DateTime RentalDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration);
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }
    }
}