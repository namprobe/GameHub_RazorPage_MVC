using AutoMapper;
using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.BLL.QueryBuilders;
using GameHub.DAL.Common;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameHub.BLL.Implements;

public class DeveloperService : IDeveloperService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeveloperService> _logger;
    private readonly CurrentUserHelper _currentUserHelper;
    private readonly DeveloperQueryBuilder _developerQueryBuilder;
    private readonly IMapper _mapper;

    public DeveloperService(
        IUnitOfWork unitOfWork, 
        ILogger<DeveloperService> logger, 
        CurrentUserHelper currentUserHelper, 
        DeveloperQueryBuilder developerQueryBuilder,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserHelper = currentUserHelper;
        _developerQueryBuilder = developerQueryBuilder;
        _mapper = mapper;
    }

    public async Task<Result> CreateAsync(DeveloperRequest request)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateAdminUserAsync();
            if (!success)
            {
                return Result.Error(error!);
            }
            
            var currentUserId = _currentUserHelper.GetCurrentUserId();
            var developer = _mapper.Map<Developer>(request);
            
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                developer.InitializeAudit(currentUserId);
                await _unitOfWork.DeveloperRepository.AddAsync(developer);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            
            return Result.Success("Create Developer successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating Developer");
            return Result.Error("Create Developer failed");
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
            
            var developer = await _unitOfWork.DeveloperRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (developer == null)
            {
                return Result.Error("Developer not found");
            }
            
            // Check if developer has associated games
            if (await _unitOfWork.GameRepository.AnyAsync(x => x.DeveloperId == id && x.IsActive))
            {
                return Result.Error("Developer is associated with active games");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.DeveloperRepository.Delete(developer);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            
            return Result.Success("Delete Developer successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting Developer");
            return Result.Error("Delete Developer failed");
        }
    }

    public async Task<Result<IEnumerable<DeveloperItem>>> GetAllAsync()
    {
        try
        {
            var developers = await _unitOfWork.DeveloperRepository.FindAsync(x => x.IsActive);
            return Result<IEnumerable<DeveloperItem>>.Success(_mapper.Map<IEnumerable<DeveloperItem>>(developers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting all Developers");
            return Result<IEnumerable<DeveloperItem>>.Error("Get all Developers failed");
        }
    }

    public async Task<Result<DeveloperResponse>> GetByIdAsync(int id)
    {
        try
        {
            var developer = await _unitOfWork.DeveloperRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (developer == null)
            {
                return Result<DeveloperResponse>.Error("Developer not found");
            }
            
            return Result<DeveloperResponse>.Success(_mapper.Map<DeveloperResponse>(developer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting Developer by id");
            return Result<DeveloperResponse>.Error("Get Developer by id failed");
        }
    }

    public async Task<PaginationResult<DeveloperResponse>> GetPagedAsync(DeveloperFilter filter)
    {
        try
        {
            // Build predicate using query builder
            var predicate = _developerQueryBuilder.BuildPredicate(filter);
            var orderBy = _developerQueryBuilder.BuildOrderBy(filter);
            
            // Use GetPagedAsync
            var (developers, totalCount) = await _unitOfWork.DeveloperRepository.GetPagedAsync(
                pageNumber: filter.Page,
                pageSize: filter.PageSize,
                predicate: predicate,
                orderBy: orderBy?? (o => o.CreatedAt!),
                isAscending: filter.IsAscending ?? false // Mặc định DESC để hiển thị mới nhất trước
            );
            
            var mappedDevelopers = _mapper.Map<List<DeveloperResponse>>(developers.ToList());
            
            return PaginationResult<DeveloperResponse>.Success(
                mappedDevelopers, 
                filter.Page, 
                filter.PageSize, 
                totalCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting paged Developers");
            return PaginationResult<DeveloperResponse>.Error("Get paged Developers failed");
        }
    }

    public async Task<Result> UpdateAsync(int id, DeveloperRequest request)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateAdminUserAsync();
            if (!success)
            {
                return Result.Error(error!);
            }
            
            var oldDeveloper = await _unitOfWork.DeveloperRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (oldDeveloper == null)
            {
                return Result.Error("Developer not found");
            }
            
            var currentUserId = _currentUserHelper.GetCurrentUserId();
            
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var developerToUpdate = _mapper.Map<Developer>(request);
                developerToUpdate.Id = id;
                developerToUpdate.UpdateAudit(currentUserId, oldDeveloper);
                _unitOfWork.DeveloperRepository.Update(developerToUpdate);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            
            return Result.Success("Update Developer successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating Developer");
            return Result.Error("Update Developer failed");
        }
    }
}
