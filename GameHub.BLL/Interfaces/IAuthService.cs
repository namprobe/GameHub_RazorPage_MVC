using GameHub.BLL.DTOs.Auth;
using GameHub.BLL.Models;

namespace GameHub.BLL.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result<ProfileResponse>> GetProfileAsync();
    Task<Result> UpdateProfileAsync(UpdateProfileRequest request);
    Task<Result> ChangePasswordAsync(ChangePasswordRequest request);
}