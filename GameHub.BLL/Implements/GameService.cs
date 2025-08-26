using AutoMapper;
using GameHub.BLL.DTOs.Game;
using GameHub.BLL.Helpers;
using GameHub.BLL.Hubs;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.BLL.QueryBuilders;
using GameHub.DAL.Common;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GameHub.BLL.Implements;

public class GameService : IGameService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GameService> _logger;
    private readonly CurrentUserHelper _currentUserHelper;
    private readonly GameQueryBuilder _gameQueryBuilder;
    private readonly IMapper _mapper;
    private readonly IHubContext<GameHub.BLL.Hubs.GameHub> _hubContext;
    private readonly FileUploadHelper _fileUploadHelper;

    public GameService(IUnitOfWork unitOfWork, ILogger<GameService> logger, CurrentUserHelper currentUserHelper, IMapper mapper, GameQueryBuilder gameQueryBuilder, IHubContext<GameHub.BLL.Hubs.GameHub> hubContext, FileUploadHelper fileUploadHelper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserHelper = currentUserHelper;
        _mapper = mapper;
        _gameQueryBuilder = gameQueryBuilder;
        _hubContext = hubContext;
        _fileUploadHelper = fileUploadHelper;
    }

    public async Task<Result> CreateAsync(GameRequest request)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateAdminUserAsync();
            if (!success)
            {
                return Result.Error(error!);
            }
            var currentUserId = _currentUserHelper.GetCurrentUserId();
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var game = _mapper.Map<Game>(request);
                
                // Handle image upload
                if (request.Image != null)
                {
                    var imagePath = await _fileUploadHelper.UploadFileAsync(request.Image, "games");
                    game.ImagePath = imagePath;
                }
                
                game.InitializeAudit(currentUserId);
                await _unitOfWork.GameRepository.AddAsync(game);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            _logger.LogInformation("Game created successfully by user {UserId}", currentUserId);
            return Result.Success("Create Game successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating Game");
            return Result.Error("Create Game failed");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateAdminUserAsync();
            if (!success)
            {
                return Result.Error(error!);
            }
            var game = await _unitOfWork.GameRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (game == null)
            {
                return Result.Error("Game not found");
            }
            if (await _unitOfWork.GameRegistrationRepository.AnyAsync(x => x.GameRegistrationDetails.Any(gd => gd.GameId == id) && x.IsActive))
            {
                return Result.Error("Game is associated with valid game registrations");
            }

            // Store image path for deletion
            var imagePath = game.ImagePath;
            
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.GameRepository.Delete(game);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            
            // Delete image file asynchronously AFTER transaction is committed
            if (!string.IsNullOrEmpty(imagePath))
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        _fileUploadHelper.DeleteFile(imagePath);
                        _logger.LogInformation("Game image deleted: {ImagePath}", imagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete game image: {ImagePath}", imagePath);
                    }
                });
            }
            
            await _hubContext.Clients.All.SendAsync("GameDeleted", game.Id);
            return Result.Success("Delete Game successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting Game");
            return Result.Error("Delete Game failed");
        }
    }

    public async Task<Result<GameResponse>> GetByIdAsync(int id)
    {
        try
        {
            var game = await _unitOfWork.GameRepository.GetFirstOrDefaultAsync(
                x => x.Id == id, 
                x => x.Developer!, 
                x => x.Category!
            );
            if (game == null)
            {
                return Result<GameResponse>.Error("Game not found");
            }

            var gameResponse = _mapper.Map<GameResponse>(game);
            return Result<GameResponse>.Success(gameResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting Game by id");
            return Result<GameResponse>.Error("Get Game by id failed");
        }
    }

    public async Task<PaginationResult<GameResponse>> GetPagedAsync(GameFilter filter)
    {
        try
        {
            // Build predicate using query builder
            var predicate = _gameQueryBuilder.BuildPredicate(filter);
            var orderBy = _gameQueryBuilder.BuildOrderBy(filter);
            
            // Use GetPagedAsync with navigation properties included
            var (games, totalCount) = await _unitOfWork.GameRepository.GetPagedAsync(
                pageNumber: filter.Page,
                pageSize: filter.PageSize,
                predicate: predicate,
                orderBy: orderBy?? (o => o.CreatedAt!),
                isAscending: filter.IsAscending ?? false, // Mặc định DESC để hiển thị mới nhất trước
                // Include navigation properties for search and mapping
                x => x.Developer!,
                x => x.Category!
            );
            
            var mappedGames = _mapper.Map<List<GameResponse>>(games.ToList());
            return PaginationResult<GameResponse>.Success(
                mappedGames, 
                filter.Page, 
                filter.PageSize, 
                totalCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting paged Games");
            return PaginationResult<GameResponse>.Error("Get paged Games failed");
        }
    }

    public async Task<Result> UpdateAsync(int id, GameRequest request)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateAdminUserAsync();
            if (!success)
            {
                return Result.Error(error!);
            }
            var oldGame = await _unitOfWork.GameRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (oldGame == null)
            {
                return Result.Error("Game not found");
            }
            var currentUserId = _currentUserHelper.GetCurrentUserId();
            var gameToUpdate = _mapper.Map<Game>(request);
            
            // Store old image path for deletion
            var oldImagePath = oldGame.ImagePath;
            
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                // Handle image upload
                if (request.Image != null)
                {
                    var imagePath = await _fileUploadHelper.UploadFileAsync(request.Image, "games");
                    gameToUpdate.ImagePath = imagePath;
                }
                else
                {
                    // Keep existing image path if no new image uploaded
                    gameToUpdate.ImagePath = oldGame.ImagePath;
                }
                
                gameToUpdate.Id = id;
                gameToUpdate.UpdateAudit(currentUserId, oldGame);
                _unitOfWork.GameRepository.Update(gameToUpdate);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            
            // Delete old image asynchronously AFTER transaction is committed
            if (request.Image != null && !string.IsNullOrEmpty(oldImagePath))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        _fileUploadHelper.DeleteFile(oldImagePath);
                        _logger.LogInformation("Old game image deleted: {ImagePath}", oldImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete old game image: {ImagePath}", oldImagePath);
                    }
                });
            }
            await _hubContext.Clients.All.SendAsync("GameUpdated", gameToUpdate.Id);
            _logger.LogInformation("Game updated successfully by user {UserId}", currentUserId);
            return Result.Success("Update Game successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating Game");
            return Result.Error("Update Game failed");
        }
    }

    /// <summary>
    /// Increment the registration count for a game when a new player registers
    /// </summary>
    /// <param name="gameId">The ID of the game</param>
    /// <returns>Success result</returns>
    public async Task<Result> IncrementRegistrationCountAsync(int gameId)
    {
        try
        {
            var game = await _unitOfWork.GameRepository.GetFirstOrDefaultAsync(x => x.Id == gameId);
            if (game == null)
            {
                return Result.Error("Game not found");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                game.RegistrationCount++;
                _unitOfWork.GameRepository.Update(game);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            _logger.LogInformation("Registration count incremented for game {GameId}. New count: {Count}", gameId, game.RegistrationCount);
            return Result.Success("Registration count incremented successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while incrementing registration count for game {GameId}", gameId);
            return Result.Error("Failed to increment registration count");
        }
    }
}
