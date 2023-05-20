namespace Bookify.Domain.Entities
{
    public class Subscription
    {
        public int Id { get; set; }

        public int SubscriberId { get; set; }

        public Subscriber? Subscriber { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? CreatedById { get; set; }

        public ApplicationUser? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}