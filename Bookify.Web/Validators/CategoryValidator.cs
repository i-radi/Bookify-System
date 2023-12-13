using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Bookify.Web.Validators;

public class CategoryValidator : AbstractValidator<CategoryFormViewModel>
{
    public CategoryValidator(IStringLocalizer<CategoryValidator> localizer)
    {
        RuleFor(x => x.NameInEnglish)
            .MaximumLength(100).WithName(localizer["category"]).WithMessage(localizer["maxLengthError"])
            .Matches(RegexPatterns.CharactersOnly_Eng).WithMessage(localizer["onlyEnglishLettersAreAllowedError"]);

        RuleFor(x => x.NameInArabic)
            .MaximumLength(100).WithName(localizer["category"]).WithMessage(localizer["maxLengthError"])
            .Matches(RegexPatterns.CharactersOnly_Ar).WithMessage(localizer["onlyArabicLettersAreAllowedError"]);
    }
}