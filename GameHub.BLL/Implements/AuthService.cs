using GameHub.BLL.DTOs.Auth;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.DAL.Common;
using GameHub.DAL.Entities;
using GameHub.DAL.Enums;
using GameHub.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GameHub.BLL.Implements;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SessionHelper _sessionHelper;
    private readonly FileUploadHelper _fileUploadHelper;
    private readonly CurrentUserHelper _currentUserHelper;

    public AuthService(IUnitOfWork unitOfWork, ILogger<AuthService> logger, IConfiguration configuration, SessionHelper sessionHelper, FileUploadHelper fileUploadHelper, CurrentUserHelper currentUserHelper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;
        _sessionHelper = sessionHelper;
        _fileUploadHelper = fileUploadHelper;
        _currentUserHelper = currentUserHelper;
    }
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(x => x.Email == request.Email);
            if (user == null)
            {
                return Result<LoginResponse>.Error("User not found");
            }

            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Result<LoginResponse>.Error("Invalid password");
            }

            if (user.IsActive == false)
            {
                return Result<LoginResponse>.Error("User is currently blocked");
            }

            var (accessToken, expiresAt) = JwtHelper.GenerateToken(user, _configuration);

            if (user.Role == RoleEnum.Player)
            {
                var player = await _unitOfWork.PlayerRepository.GetFirstOrDefaultAsync(x => x.UserId == user.Id);
                if (player != null)
                {
                    try
                    {
                        player.LastLogin = DateTime.Now;
                        player.UpdateAudit(user.Id);
                        _unitOfWork.PlayerRepository.Update(player);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        //continue login process, no response error result
                        _logger.LogError(ex, "Error updating player last login");
                    }
                }
            }
            _sessionHelper.SetJwtToken(accessToken, expiresAt);
            return Result<LoginResponse>.Success(new LoginResponse { AccessToken = accessToken, ExpiresAt = expiresAt, Role = user.Role.ToString() }, "Login successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in");
            return Result<LoginResponse>.Error("An error occurred while logging in", new List<string> { ex.Message });
        }
    }

    public async Task<Result<ProfileResponse>> GetProfileAsync()
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateCurrentUserAsync();
            if (!success)
            {
                return Result<ProfileResponse>.Error(error!);
            }

            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(x => x.Id == _currentUserHelper.GetCurrentUserId(), x => x.Player!);
            if (user == null)
            {
                return Result<ProfileResponse>.Error("User not found");
            }

            var profile = new ProfileResponse
            {
                Username = user.Player!.Username?? "Unknown",
                Email = user.Email,
                AvatarPath = user.Player!.AvatarPath,
                CreatedAt = user.CreatedAt,
                LastLogin = user.Player!.LastLogin
            };

            return Result<ProfileResponse>.Success(profile, "Profile retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile");
            return Result<ProfileResponse>.Error("An error occurred while retrieving profile", new List<string> { ex.Message });
        }
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var isEmailExists = await _unitOfWork.UserRepository.AnyAsync(x => x.Email == request.Email);
            if (isEmailExists)
            {
                return Result.Error("User with this email already exists");
            }

            var isUsernameExists = await _unitOfWork.PlayerRepository.AnyAsync(x => x.Username == request.Username);
            if (isUsernameExists)
            {
                return Result.Error("User with this username already exists");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                IsActive = true,
                Role = RoleEnum.Player,
                JoinDate = DateTime.Now

            };

            var player = new Player
            {
                Username = request.Username,
            };

            if (request.Avatar != null)
            {
                try
                {
                    var avatarPath = await _fileUploadHelper.UploadFileAsync(request.Avatar, "avatars");
                    if (avatarPath != null)
                    {
                        player.AvatarPath = avatarPath;
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi avatar nhưng không rollback
                    _logger.LogError(ex, "Error uploading avatar for user {Email}. Registration will proceed without avatar.", request.Email);
                }
            }

            user.Player = player;
            player.User = user;

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                user.InitializeAudit();
                player.InitializeAudit();
                await _unitOfWork.UserRepository.AddAsync(user);
                await _unitOfWork.PlayerRepository.AddAsync(player);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            //Nếu fail ko rollback, ko chặn luồng chính
            _ = Task.Run(async () =>
            {
                var cart = new Cart
                {
                    PlayerId = player.Id,
                };
                cart.InitializeAudit(user.Id);
                await _unitOfWork.CartRepository.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            });

            return Result.Success("User registered successfully");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return Result.Error("An error occurred while registering user", new List<string> { ex.Message });
        }
    }

    public async Task<Result> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateCurrentUserAsync();
            if (!success)
            {
                return Result.Error(error!);
            }

            var userId = _currentUserHelper.GetCurrentUserId();

            var player = await _unitOfWork.PlayerRepository.GetFirstOrDefaultAsync(x => x.UserId == userId);
            if (player == null)
            {
                return Result.Error("Player not found");
            }

            if (await _unitOfWork.PlayerRepository.AnyAsync(x => x.Username == request.Username && x.Id != player.Id))
            {
                return Result.Error("Username already exists");
            }

            if (request.Avatar != null)
            {
                try
                {
                    var avatarPath = await _fileUploadHelper.UploadFileAsync(request.Avatar, "avatars");
                    if (avatarPath != null)
                    {
                        // Dùng background task để xóa avatar cũ khi upload avatar mới
                        _ = Task.Run(() =>
                        {
                            var oldAvatarPath = player.AvatarPath;
                            if (oldAvatarPath != null)
                            {
                                _fileUploadHelper.DeleteFile(oldAvatarPath);
                            }
                        });
                        player.AvatarPath = avatarPath;
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi avatar nhưng không rollback
                    _logger.LogError(ex, "Error uploading avatar.");
                }
            }
            player.Username = request.Username;
            try 
            {
                await _unitOfWork.BeginTransactionAsync();
                player.UpdateAudit(userId);
                _unitOfWork.PlayerRepository.Update(player);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            return Result.Success("Profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return Result.Error("An error occurred while updating profile");
        }
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateCurrentUserAsync();
            if (!success)
            {
                return Result.Error(error!);
            }

            var userId = _currentUserHelper.GetCurrentUserId();
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return Result.Error("User not found");
            }

            if (!PasswordHelper.VerifyPassword(request.OldPassword, user.PasswordHash))
            {
                return Result.Error("Invalid old password");
            }

            user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                user.UpdateAudit(userId);
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            return Result.Success("Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return Result.Error("An error occurred while changing password");
        }
    }
}