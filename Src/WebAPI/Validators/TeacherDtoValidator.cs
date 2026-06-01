namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class TeacherDtoValidator : AbstractValidator<TeacherDto>
{
    public TeacherDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .Must(x => x == null || x.Length >= 2)
            .WithMessage("Firstname must be null or at least 2 characters long.");
        RuleFor(x => x.LastName)
            .MinimumLength(3)
            .WithMessage("Lastname must be at least 2 characters long");
    }
}