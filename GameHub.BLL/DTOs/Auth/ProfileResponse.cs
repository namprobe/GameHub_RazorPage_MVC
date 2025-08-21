namespace GameHub.BLL.DTOs.Auth;

public class ProfileResponse
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? AvatarPath { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}