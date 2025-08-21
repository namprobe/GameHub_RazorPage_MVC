using System.ComponentModel.DataAnnotations;

namespace GameHub.BLL.Validations;

public class GameTitleValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string title)
            return false;
            
        // Kiểm tra độ dài không quá 14 ký tự
        if (string.IsNullOrWhiteSpace(title) || title.Length > 14)
        {
            ErrorMessage = "Game title must not be empty and not exceed 14 characters.";
            return false;
        }
        
        // Kiểm tra ký tự đầu là chữ hoa
        if (!char.IsUpper(title[0]))
        {
            ErrorMessage = "Game title must start with an uppercase letter.";
            return false;
        }
        
        return true;
    }
}
