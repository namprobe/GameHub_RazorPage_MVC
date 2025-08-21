using GameHub.BLL.DTOs.Game;
using GameHub.BLL.Models;

namespace GameHub.BLL.Interfaces;

public interface IGameService
{
    Task<Result<GameResponse>> GetByIdAsync(int id);
    Task<Result> CreateAsync(GameRequest request);
    Task<Result> UpdateAsync(int id, GameRequest request);
    Task<Result> DeleteAsync(int id);
    Task<PaginationResult<GameResponse>> GetPagedAsync(GameFilter filter);
}
