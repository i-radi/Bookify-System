namespace Bookify.Web.Validators;

public class BookValidator : AbstractValidator<BookFormViewModel>
{
    public BookValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(500).WithMessage(Errors.MaxLength);

        RuleFor(x => x.Publisher)
            .MaximumLength(200).WithMessage(Errors.MaxLength);

        RuleFor(x => x.Hall)
            .MaximumLength(50).WithMessage(Errors.MaxLength);
    }
}