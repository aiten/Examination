namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class ExamDtoValidator : AbstractValidator<ExamDto>
{
    public ExamDtoValidator()
    {
        RuleFor(x => x.Description)
            .MinimumLength(1)
            .WithMessage("Description must be at least 1 characters long");

        RuleFor(x => x.From)
            .LessThan(x => x.To)
            .WithMessage("From time must be before To time.");

        RuleFor(x => x.Pin)
            .InclusiveBetween(100, 999)
            .When(x => x.Pin.HasValue)
            .WithMessage("Pin must be between 100 and 999.");
    }
}