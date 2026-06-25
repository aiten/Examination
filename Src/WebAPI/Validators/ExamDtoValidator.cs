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
            .Matches(@"^\d{5}$")
            .When(x => x.Pin != null)
            .WithMessage("Pin must be exactly 5 digits.");
    }
}