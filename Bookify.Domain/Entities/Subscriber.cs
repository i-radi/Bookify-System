namespace Bookify.Domain.Entities
{
    public class Subscriber : BaseEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string NationalId { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public bool HasWhatsApp { get; set; }
        public string Email { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string ImageThumbnailUrl { get; set; } = null!;
        public int AreaId { get; set; }
        public Area? Area { get; set; }
        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }
        public string Address { get; set; } = null!;
        public bool IsBlackListed { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}