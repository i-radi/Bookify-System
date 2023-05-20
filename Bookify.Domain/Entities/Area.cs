namespace Bookify.Domain.Entities
{
    [Index(nameof(Name), nameof(GovernorateId), IsUnique = true)]
    public class Area : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }
    }
}
