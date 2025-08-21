using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GameHub.BLL.Validations;

public class ImageAttributeValidation : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not IFormFile file)
        {
            ErrorMessage = "Invalid file type";
            return false;
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            ErrorMessage = "File size must be less than 10MB";
            return false;
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            ErrorMessage = "Invalid image file type";
            return false;
        }
        return true;
    }
}