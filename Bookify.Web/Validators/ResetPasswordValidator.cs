namespace Bookify.Web.Validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordFormViewModel>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Password)
            .Length(8, 100).WithMessage(Errors.MaxMinLength)
            .Matches(RegexPatterns.Password).WithMessage(Errors.WeakPassword);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage(Errors.ConfirmPasswordNotMatch);
    }
}