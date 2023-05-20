using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class SubscriberFormViewModel
    {
        public string? Key { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Display(Name = "Date Of Birth")]
        [AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.NotAllowFutureDates)]
        public DateTime DateOfBirth { get; set; } = DateTime.Now;

        [Display(Name = "National ID")]
        [Remote("AllowNationalId", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string NationalId { get; set; } = null!;

        [Display(Name = "Mobile Number")]
        [Remote("AllowMobileNumber", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string MobileNumber { get; set; } = null!;

        public bool HasWhatsApp { get; set; }

        [Remote("AllowEmail", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [RequiredIf("Key == ''", ErrorMessage = Errors.EmptyImage)]
        public IFormFile? Image { get; set; }

        [Display(Name = "Area")]
        public int AreaId { get; set; }

        public IEnumerable<SelectListItem>? Areas { get; set; } = new List<SelectListItem>();

        [Display(Name = "Governorate")]
        public int GovernorateId { get; set; }

        public IEnumerable<SelectListItem>? Governorates { get; set; }

        public string Address { get; set; } = null!;

        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
    }
}