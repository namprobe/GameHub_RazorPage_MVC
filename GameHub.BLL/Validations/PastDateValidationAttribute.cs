using System.ComponentModel.DataAnnotations;

namespace GameHub.BLL.Validations;

public class PastDateValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true; // Allow null values if not marked as Required
            
        if (value is DateOnly dateOnly)
        {
            if (dateOnly >= DateOnly.FromDateTime(DateTime.Now))
            {
                ErrorMessage = "Release date must be before today.";
                return false;
            }
            return true;
        }
        
        if (value is DateTime dateTime)
        {
            if (dateTime.Date >= DateTime.Now.Date)
            {
                ErrorMessage = "Release date must be before today.";
                return false;
            }
            return true;
        }
        
        return false;
    }
}
