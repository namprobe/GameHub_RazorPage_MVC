using System.ComponentModel.DataAnnotations;

namespace GameHub.BLL.DTOs.Auth;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Old password is required")]
    public string OldPassword { get; set; } = null!;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string NewPassword { get; set; } = null!;
}