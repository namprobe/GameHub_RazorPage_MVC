using GameHub.BLL.DTOs.Cart;
using GameHub.BLL.Models;

namespace GameHub.BLL.Interfaces;

public interface ICartService
{
    Task<Result<CartResponse>> GetCurrentCartAsync(BasePaginationFilter filter);
    Task<Result<List<CartItemResponse>>> GetAllCurrentCartItemsAsync();
    Task<Result> AddToCartAsync(int gameId);
    Task<Result> RemoveFromCartAsync(int gameId);
    Task<Result> ClearCartAsync();
    Task<bool> IsInCurrentCartAsync(int gameId);
    Task<bool> IsGameOwnedByCurrentPlayerAsync(int gameId);
}