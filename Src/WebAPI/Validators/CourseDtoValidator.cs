namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class CourseDtoValidator : AbstractValidator<CourseDto>
{
    public CourseDtoValidator()
    {
        RuleFor(x => x.Pin)
            .InclusiveBetween(10000, 99999)
            .When(x => x.Pin.HasValue)
            .WithMessage("Pin must be between 10000 and 99999.");
    }
}
