namespace GameHub.BLL.DTOs.Auth;
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? Role { get; set; }
}