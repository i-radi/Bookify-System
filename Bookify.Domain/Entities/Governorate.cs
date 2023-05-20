namespace Bookify.Domain.Entities
{
    public class Governorate : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Area> Areas { get; set; } = new List<Area>();
    }
}