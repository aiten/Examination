namespace WebAPI.Validators;

using FluentValidation;

using WebAPI.Endpoints;

public class StudentExamResultQueryDtoValidator : AbstractValidator<StudentExamResultQueryDto>
{
    public StudentExamResultQueryDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("FirstName must not be empty.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("LastName must not be empty.");

        RuleFor(x => x.Pin)
            .InclusiveBetween(10000, 99999)
            .WithMessage("Pin must be between 10000 and 99999.");

        RuleFor(x => x.RegistrationCode)
            .MinimumLength(5)
            .WithMessage("RegistrationCode must not be empty.");
    }
}