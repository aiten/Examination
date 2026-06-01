namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class SubtaskDtoValidator : AbstractValidator<SubtaskDto>
{
    public SubtaskDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(256)
            .WithMessage("Description must not be empty and at most 256 characters.");

        RuleFor(x => x.Points)
            .GreaterThanOrEqualTo(0)
            .Unless(x => x.Bonus == true)
            .WithMessage("Points must be greater or equal than 0.");
    }
}
