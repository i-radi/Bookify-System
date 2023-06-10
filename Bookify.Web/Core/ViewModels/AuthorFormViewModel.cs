namespace Bookify.Web.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Author")]
        [Remote("AllowItem", null!,
            AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; } = null!;
    }
}