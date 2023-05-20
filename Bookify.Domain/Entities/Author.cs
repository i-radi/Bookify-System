namespace Bookify.Domain.Entities
{
    public class Author : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}