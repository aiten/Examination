namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class ClassDtoValidator : AbstractValidator<ClassDto>
{
    public ClassDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description must not be empty.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1980, 2035)
            .WithMessage("Year must be between 1980 and 2035.");
    }
}