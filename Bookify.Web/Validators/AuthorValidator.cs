namespace Bookify.Web.Validators;

public class AuthorValidator : AbstractValidator<AuthorFormViewModel>
{
    public AuthorValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage(Errors.MaxLength)
            .Matches(RegexPatterns.CharactersOnly_Eng).WithMessage(Errors.OnlyEnglishLetters);
    }
}