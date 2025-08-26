using GameHub.BLL.DTOs.Cart;
using GameHub.BLL.Models;

namespace GameHub.BLL.Interfaces;

public interface ICartService
{
    Task<Result<CartResponse>> GetCurrentCartAsync();
    Task<Result> AddToCartAsync(CartItemRequest request);
    Task<Result> RemoveFromCartAsync(int gameId);
    Task<Result> ClearCartAsync();
}