using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GameHub.BLL.Validations;

public class ImageAttributeValidation : ValidationAttribute
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly int _maxFileSize = 10 * 1024 * 1024; // 10MB

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Image is optional
        }

        if (value is not IFormFile file)
        {
            return new ValidationResult("Invalid file type");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return new ValidationResult($"Only {string.Join(", ", _allowedExtensions)} files are allowed");
        }

        // Check file size
        if (file.Length > _maxFileSize)
        {
            return new ValidationResult($"File size must be less than {_maxFileSize / (1024 * 1024)}MB");
        }

        return ValidationResult.Success;
    }
}