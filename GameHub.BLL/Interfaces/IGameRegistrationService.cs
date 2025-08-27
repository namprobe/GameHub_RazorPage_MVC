using GameHub.BLL.DTOs.GameRegistration;
using GameHub.BLL.Models;
using Microsoft.AspNetCore.Http;

namespace GameHub.BLL.Interfaces;

public interface IGameRegistrationService
{
    Task<Result<string>> RegisterGameAsync(GameRegistrationRequest request);
    Task<Result<GameRegistrationResponse>> GetByIdAsync(int id);
    Task<Result> ToggleStatusAsync(int id);
    Task<PaginationResult<GameRegistrationResponse>> GetPagedAsync(GameRegistrationFilter filter);
    Task<Result> HandleVnPayReturnAsync(IQueryCollection queryParams);
}