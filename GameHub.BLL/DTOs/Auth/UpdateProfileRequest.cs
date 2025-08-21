using System.ComponentModel.DataAnnotations;
using GameHub.BLL.Validations;
using Microsoft.AspNetCore.Http;

namespace GameHub.BLL.DTOs.Auth;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
    public string Username { get; set; } = null!;

    [ImageAttributeValidation]
    public IFormFile? Avatar { get; set; }
}