using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;
using GameHub.DAL.Enums;

namespace GameHub.BLL.Helpers;

public class CurrentUserHelper
{
    private readonly SessionHelper _sessionHelper;
    private readonly IUnitOfWork _unitOfWork;

    public CurrentUserHelper(SessionHelper sessionHelper, IUnitOfWork unitOfWork)
    {
        _sessionHelper = sessionHelper;
        _unitOfWork = unitOfWork;
    }
    

    public int GetCurrentUserId()
    {
        var userId = _sessionHelper.GetCurrentUserId();
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User not logged in or session expired");
        }
        return userId.Value;
    }

    public bool IsLoggedIn()
    {
        return _sessionHelper.IsJwtValid();
    }

    public async Task<(bool success, string? error)> ValidateCurrentUserAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userExists = await _unitOfWork.UserRepository.AnyAsync(x => x.Id == userId && x.IsActive);
            
            if (!userExists)
            {
                return (false, "User not found or account is blocked");
            }

            return (true, null);
        } 
        catch
        {
            return (false, $"User validation failed");
            throw;
        }
    }

    public string GetCurrentRole()
    {
        var role = _sessionHelper.GetCurrentRole();
        if (string.IsNullOrEmpty(role))
        {
            throw new UnauthorizedAccessException("Role not found");
        }
        return role;
    }

    public async Task<(bool success, string? error)> ValidateAdminUserAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var isAdminUser = await _unitOfWork.UserRepository.AnyAsync(x => x.Id == userId && x.IsActive && x.Role == RoleEnum.Admin);
            
            if (!isAdminUser)
            {
                return (false, "Access denied: Admin role required or account is blocked");
            }

            return (true, null);
        }
        catch 
        {
            return (false, "Admin validation failed");
            throw;
        }
    }

    public async Task<bool> IsCurrentUserAdminAsync()
    {
        var (success, _) = await ValidateAdminUserAsync();
        return success;
    }

    public async Task<(bool success, string? error)> ValidatePlayerUserAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var isPlayerUser = await _unitOfWork.UserRepository.AnyAsync(x => x.Id == userId && x.IsActive && x.Role == RoleEnum.Player);

            if (!isPlayerUser)
            {
                return (false, "Access denied: Player role required or account is blocked");
            }

            return (true, null);
        }
        catch
        {
            return (false, "Player validation failed");
            throw;
        }
    }

    public async Task<bool> IsCurrentUserPlayerAsync()
    {
        var (success, _) = await ValidatePlayerUserAsync();
        return success;
    }

    public void Logout()
    {
        _sessionHelper.ClearSession();
    }
}
