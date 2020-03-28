using System.ComponentModel.DataAnnotations;

namespace NuGet.Services.Calculator.Logic
{
    public class PackageIdValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var validated = InputValidator.PackageId(value);
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
