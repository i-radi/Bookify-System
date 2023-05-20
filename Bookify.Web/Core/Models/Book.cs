namespace Bookify.Web.Core.Models
{
    [Index(nameof(Title), nameof(AuthorId), IsUnique = true)]
    public class Book : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public string Title { get; set; } = null!;

        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        [MaxLength(200)]
        public string Publisher { get; set; } = null!;

        public DateTime PublishingDate { get; set; }

        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; }

        [MaxLength(50)]
        public string Hall { get; set; } = null!;

        public bool IsAvailableForRental { get; set; }

        public string Description { get; set; } = null!;

        public ICollection<BookCategory> Categories { get; set; } = new List<BookCategory>();
        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
    }
}