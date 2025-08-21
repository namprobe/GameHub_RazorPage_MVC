using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.Models;

namespace GameHub.BLL.Interfaces;

public interface IGameCategoryService
{
    Task<PaginationResult<GameCategoryResponse>> GetPagedAsync(GameCategoryFilter filter);
    Task<Result<IEnumerable<GameCategoryItem>>> GetAllAsync();
    Task<Result<GameCategoryResponse>> GetByIdAsync(int id);
    Task<Result> CreateAsync(GameCategoryRequest request);
    Task<Result> UpdateAsync(int id, GameCategoryRequest request);
    Task<Result> DeleteAsync(int id);
}