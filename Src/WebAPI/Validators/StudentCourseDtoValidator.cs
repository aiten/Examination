namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class StudentCourseDtoValidator : AbstractValidator<StudentCourseDto>
{
    public StudentCourseDtoValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0)
            .WithMessage("StudentId must be a valid student.");

        RuleFor(x => x.RegistrationCode)
            .MaximumLength(5)
            .WithMessage("RegistrationCode must be at most 5 characters.");
    }
}
