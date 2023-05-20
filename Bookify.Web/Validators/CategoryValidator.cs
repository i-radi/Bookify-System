namespace Bookify.Web.Validators;

public class CategoryValidator : AbstractValidator<CategoryFormViewModel>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage(Errors.MaxLength)
            .Matches(RegexPatterns.CharactersOnly_Eng).WithMessage(Errors.OnlyEnglishLetters);
    }
}