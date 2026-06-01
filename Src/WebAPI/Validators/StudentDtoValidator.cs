namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class StudentDtoValidator : AbstractValidator<StudentDto>
{
    public StudentDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("FirstName must not be empty and at most 64 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("LastName must not be empty and at most 64 characters.");
    }
}
