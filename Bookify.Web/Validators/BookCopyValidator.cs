namespace Bookify.Web.Validators;

public class BookCopyValidator : AbstractValidator<BookCopyFormViewModel>
{
    public BookCopyValidator()
    {
        RuleFor(x => x.EditionNumber)
            .InclusiveBetween(1, 1000).WithMessage(Errors.InvalidRange);
    }
}