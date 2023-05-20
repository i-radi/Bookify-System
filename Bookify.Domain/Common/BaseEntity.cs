using Bookify.Domain.Entities;

namespace Bookify.Domain.Common
{
    public class BaseEntity
    {
        public bool IsDeleted { get; set; }
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? LastUpdatedById { get; set; }
        public ApplicationUser? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    }
}