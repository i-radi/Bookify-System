namespace Bookify.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "categoryInEnglish"),
            Required(ErrorMessage = "requiredField")]
        [Remote("AllowEnglishItem", null!, AdditionalFields = "Id", ErrorMessage = "duplicatedError")]
        public string NameInEnglish { get; set; } = null!;

        [Display(Name = "categoryInArabic"),
            Required(ErrorMessage = "requiredField")]
        [Remote("AllowArabicItem", null!, AdditionalFields = "Id", ErrorMessage = "duplicatedError")]
        public string NameInArabic { get; set; } = null!;
    }
}