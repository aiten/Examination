namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class ExamRegistrationDtoValidator : AbstractValidator<ExamRegistrationDto>
{
    public ExamRegistrationDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("FirstName must not be empty and at most 64 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("LastName must not be empty and at most 64 characters.");

        RuleFor(x => x.LoginName)
            .MaximumLength(32)
            .WithMessage("LoginName must be at most 32 characters.")
            .When(x => x.LoginName is not null);

        RuleFor(x => x.Pin)
            .InclusiveBetween(10000, 99999)
            .WithMessage("Pin must be between 10000 and 99999.");
    }
}