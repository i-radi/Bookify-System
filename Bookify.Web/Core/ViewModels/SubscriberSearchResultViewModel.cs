namespace Bookify.Web.Core.ViewModels
{
    public class SubscriberSearchResultViewModel
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? FullName { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? Email { get; set; }
        public string? NationalId { get; set; }
        public string? MobileNumber { get; set; }
    }
}