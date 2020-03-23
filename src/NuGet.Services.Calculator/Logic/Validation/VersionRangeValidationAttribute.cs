using System.ComponentModel.DataAnnotations;

namespace NuGet.Services.Calculator.Logic
{
    public class VersionRangeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var validated = InputValidator.VersionRange(value);
            if (validated.IsValid)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(validated.ErrorMessage, new[] { validationContext.MemberName });
            }
        }
    }
}
