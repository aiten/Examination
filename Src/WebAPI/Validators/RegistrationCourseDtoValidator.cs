namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class RegistrationCourseDtoValidator : AbstractValidator<RegistrationCourseDto>
{
    public RegistrationCourseDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("FirstName must not be empty and at most 64 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("LastName must not be empty and at most 64 characters.");

        RuleFor(x => x.Pin)
            .NotEmpty()
            .Matches(@"^\d{5}$")
            .WithMessage("Pin must be exactly 5 digits.");
    }
}