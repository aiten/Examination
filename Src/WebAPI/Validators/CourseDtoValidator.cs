namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class CourseDtoValidator : AbstractValidator<CourseDto>
{
    public CourseDtoValidator()
    {
        RuleFor(x => x.Pin)
            .Matches(@"^\d{5}$")
            .When(x => x.Pin != null)
            .WithMessage("Pin must be exactly 5 digits.");
    }
}
