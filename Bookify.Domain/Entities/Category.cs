namespace Bookify.Domain.Entities
{
    public class Category : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<BookCategory> Books { get; set; } = new List<BookCategory>();
    }
}