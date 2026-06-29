namespace Persistence.Validations;

using System;
using System.ComponentModel.DataAnnotations;

using Persistence.Model;

public class ExamRange : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        var to   = (TimeOnly)value;
        var exam = (Exam)validationContext.ObjectInstance;

        if ((exam.From is null || to <= exam.From))
        {
            var result = new ValidationResult("To-Time must be after From");
            return result;
        }

        return ValidationResult.Success;
    }
}