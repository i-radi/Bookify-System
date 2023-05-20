namespace Bookify.Domain.Entities
{
    public class BookCopy : BaseEntity
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public bool IsAvailableForRental { get; set; }
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; }
        public ICollection<RentalCopy> Rentals { get; set; } = new List<RentalCopy>();
    }
}