using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.BLL.Helpers;
using GameHub.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;
using GameHub.DAL.Entities;
using GameHub.DAL.Common;
using GameHub.BLL.QueryBuilders;

namespace GameHub.BLL.Implements;

public class GameCategoryService : IGameCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GameCategoryService> _logger;
    private readonly CurrentUserHelper _currentUserHelper;
    private readonly GameCategoryQueryBuilder _gameCategoryQueryBuilder;
    private readonly IMapper _mapper;

    public GameCategoryService(IUnitOfWork unitOfWork, ILogger<GameCategoryService> logger, CurrentUserHelper currentUserHelper, IMapper mapper, GameCategoryQueryBuilder gameCategoryQueryBuilder)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserHelper = currentUserHelper;
        _mapper = mapper;
        _gameCategoryQueryBuilder = gameCategoryQueryBuilder;
    }

    public async Task<Result> CreateAsync(GameCategoryRequest request)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateAdminUserAsync();
            if (!success)
            {
                return Result.Error(error!);
            }
            var currentUserId = _currentUserHelper.GetCurrentUserId();
            var gameCategory = _mapper.Map<GameCategory>(request);
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                gameCategory.InitializeAudit(currentUserId);
                await _unitOfWork.GameCategoryRepository.AddAsync(gameCategory);
                await _unitOfWork.CommitTransactionAsync();

            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            return Result.Success("Create GameCategory successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating GameCategory");
            return Result.Error("Create GameCategory failed");
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

            var gameCategory = await _unitOfWork.GameCategoryRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (gameCategory == null)
            {
                return Result.Error("GameCategory not found");
            }

            if (await _unitOfWork.GameRepository.AnyAsync(x => x.CategoryId == id))
            {
                return Result.Error("GameCategory is associated with games");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.GameCategoryRepository.Delete(gameCategory);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            return Result.Success("Delete GameCategory successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting GameCategory");
            return Result.Error("Delete GameCategory failed");
        }
    }

    public async Task<Result<IEnumerable<GameCategoryItem>>> GetAllAsync()
    {
        try
        {
            var gameCategories = await _unitOfWork.GameCategoryRepository.GetAllAsync();
            return Result<IEnumerable<GameCategoryItem>>.Success(_mapper.Map<IEnumerable<GameCategoryItem>>(gameCategories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting all GameCategory");
            return Result<IEnumerable<GameCategoryItem>>.Error("Get all GameCategory failed");
        }
    }

    public async Task<Result<GameCategoryResponse>> GetByIdAsync(int id)
    {
        try
        {
            var gameCategory = await _unitOfWork.GameCategoryRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (gameCategory == null)
            {
                return Result<GameCategoryResponse>.Error("GameCategory not found");
            }
            return Result<GameCategoryResponse>.Success(_mapper.Map<GameCategoryResponse>(gameCategory));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting GameCategory by id");
            return Result<GameCategoryResponse>.Error("Get GameCategory by id failed");
        }
    }

    public async Task<PaginationResult<GameCategoryResponse>> GetPagedAsync(GameCategoryFilter filter)
    {
        try
        {
            var query = _gameCategoryQueryBuilder.BuildPredicate(filter);
            var orderBy = _gameCategoryQueryBuilder.BuildOrderBy(filter);
            var (gameCategories, totalItems) = await _unitOfWork.GameCategoryRepository.GetPagedAsync(
                pageNumber: filter.Page,
                pageSize: filter.PageSize,
                predicate: query,
                orderBy: orderBy?? (o => o.CreatedAt!),
                isAscending: filter.IsAscending ?? false // Mặc định DESC để hiển thị mới nhất trước
            );
            return PaginationResult<GameCategoryResponse>.Success(
                _mapper.Map<List<GameCategoryResponse>>(gameCategories),
                filter.Page,
                filter.PageSize,
                totalItems
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting paged GameCategory");
            return PaginationResult<GameCategoryResponse>.Error("Get paged GameCategory failed");
        }
    }

    public async Task<Result> UpdateAsync(int id, GameCategoryRequest request)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateAdminUserAsync();
            if (!success)
            {
                return Result.Error(error!);
            }

            var oldGameCategory = await _unitOfWork.GameCategoryRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (oldGameCategory == null)
            {
                return Result.Error("GameCategory not found");
            }

            var currentUserId = _currentUserHelper.GetCurrentUserId();
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var gameCategoryToUpdate = _mapper.Map<GameCategory>(request);
                gameCategoryToUpdate.Id = id;
                gameCategoryToUpdate.UpdateAudit(currentUserId, oldGameCategory);
                _unitOfWork.GameCategoryRepository.Update(gameCategoryToUpdate);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            return Result.Success("Update GameCategory successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating GameCategory");
            return Result.Error("Update GameCategory failed");
        }
    }
}



