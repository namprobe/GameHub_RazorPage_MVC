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

    public GameService(IUnitOfWork unitOfWork, ILogger<GameService> logger, CurrentUserHelper currentUserHelper, IMapper mapper, GameQueryBuilder gameQueryBuilder, IHubContext<GameHub.BLL.Hubs.GameHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserHelper = currentUserHelper;
        _mapper = mapper;
        _gameQueryBuilder = gameQueryBuilder;
        _hubContext = hubContext;
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

            // Calculate player registration count
            var playerCount = await _unitOfWork.GameRegistrationRepository
                .CountAsync(x => x.GameRegistrationDetails.Any(gd => gd.GameId == id) && x.IsActive);

            var gameResponse = _mapper.Map<GameResponse>(game);
            gameResponse.PlayerCount = playerCount;

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
            
            // Calculate player count for each game
            foreach (var game in mappedGames)
            {
                game.PlayerCount = await _unitOfWork.GameRegistrationRepository
                    .CountAsync(x => x.GameRegistrationDetails.Any(gd => gd.GameId == game.Id) && x.IsActive);
            }
            
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
            try
            {
                await _unitOfWork.BeginTransactionAsync();
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
}
