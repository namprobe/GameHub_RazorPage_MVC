using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Models;

namespace GameHub.BLL.Interfaces;

public interface IDeveloperService
{
    Task<Result<DeveloperResponse>> GetByIdAsync(int id);
    Task<Result<IEnumerable<DeveloperItem>>> GetAllAsync();
    Task<PaginationResult<DeveloperResponse>> GetPagedAsync(DeveloperFilter filter);
    Task<Result> CreateAsync(DeveloperRequest request);
    Task<Result> UpdateAsync(int id, DeveloperRequest request);
    Task<Result> DeleteAsync(int id);
}
