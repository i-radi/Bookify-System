namespace Bookify.Web.Core.ViewModels
{
    public class BookViewModel
    {
        public int Id { get; set; }

        public string? Key { get; set; }

        public string Title { get; set; } = null!;

        public string Author { get; set; } = null!;

        public string Publisher { get; set; } = null!;

        public DateTime PublishingDate { get; set; }

        public string? ImageUrl { get; set; }

        public string? ImageThumbnailUrl { get; set; }

        public string Hall { get; set; } = null!;

        public bool IsAvailableForRental { get; set; }

        public string Description { get; set; } = null!;

        public IEnumerable<string> Categories { get; set; } = null!;

        public IEnumerable<BookCopyViewModel> Copies { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}