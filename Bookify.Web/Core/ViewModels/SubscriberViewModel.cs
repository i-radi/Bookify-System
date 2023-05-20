namespace Bookify.Web.Core.ViewModels
{
    public class SubscriberViewModel
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? NationalId { get; set; }
        public string? MobileNumber { get; set; }
        public bool? HasWhatsApp { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? Area { get; set; }
        public string? Governorate { get; set; }
        public string? Address { get; set; }
        public bool IsBlackListed { get; set; }
        public DateTime CreatedOn { get; set; }

        public IEnumerable<SubscriptionViewModel> Subscriptions { get; set; } = new List<SubscriptionViewModel>();
        public IEnumerable<RentalViewModel> Rentals { get; set; } = new List<RentalViewModel>();
    }
}