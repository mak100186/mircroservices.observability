using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class EnsureNotNullOrEmptyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {

        if (value is ICollection { Count: not 0 } collection)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("The list is null or empty.");
    }
}
