namespace WebAPI.Validators;

using FluentValidation;

using Persistence.Model;

using WebAPI.Endpoints;

public class ExamDtoValidator : AbstractValidator<ExamDto>
{
    public ExamDtoValidator()
    {
        RuleFor(x => x.Description)
            .MinimumLength(1)
            .WithMessage("Description must be at least 1 characters long");

        When(x => x.ExamType == (int)ExamType.Standard, () =>
        {
            RuleFor(x => x.Date).NotNull().WithMessage("Date is required for Standard exams.");
            RuleFor(x => x.From).NotNull().WithMessage("From time is required for Standard exams.");
            RuleFor(x => x.To).NotNull().WithMessage("To time is required for Standard exams.");
        });

        RuleFor(x => x.From)
            .LessThan(x => x.To)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("From time must be before To time.");

        RuleFor(x => x.Pin)
            .Matches(@"^\d{5}$")
            .When(x => x.Pin != null)
            .WithMessage("Pin must be exactly 5 digits.");
    }
}