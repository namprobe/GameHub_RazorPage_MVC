using AutoMapper;
using GameHub.BLL.DTOs.Cart;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.DAL.Common;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameHub.BLL.Implements;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CurrentUserHelper _currentUserHelper;
    private readonly ILogger<CartService> _logger;

    public CartService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CurrentUserHelper currentUserHelper,
        ILogger<CartService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserHelper = currentUserHelper;
        _logger = logger;
    }

    // ---------------------- Helper ----------------------
    private async Task<(Result<Player> Result, Player? Player)> GetValidatedPlayerAsync()
    {
        var result = await ValidateCurrentPlayerAsync();
        return (result, result.IsSuccess ? result.Data : null);
    }

    private async Task<Result<Player>> ValidateCurrentPlayerAsync()
    {
        if (!await _currentUserHelper.IsCurrentUserPlayerAsync())
            return Result<Player>.Error("Not authorized");

        var currentUserId = _currentUserHelper.GetCurrentUserId();
        var currentPlayer = await _unitOfWork.PlayerRepository
            .GetFirstOrDefaultAsync(x => x.UserId == currentUserId, x => x.Cart);

        return currentPlayer == null
            ? Result<Player>.Error("Player not found")
            : Result<Player>.Success(currentPlayer);
    }

    // ---------------------- Add ----------------------
    public async Task<Result> AddToCartAsync(int gameId)
    {
        try
        {
            var (validation, player) = await GetValidatedPlayerAsync();
            if (!validation.IsSuccess || player == null)
                return Result.Error(validation.Message ?? "Error adding to cart");

            var cart = player.Cart;
            if (cart == null) return Result.Error("Cart not found");

            if (!await _unitOfWork.GameRepository.AnyAsync(x => x.Id == gameId))
                return Result.Error("Game not found");

            if (await _unitOfWork.CartItemRepository.AnyAsync(x => x.CartId == cart.Id && x.GameId == gameId))
                return Result.Error("Game already in cart");

            var cartItem = new CartItem { CartId = cart.Id, GameId = gameId };
            cartItem.InitializeAudit(_currentUserHelper.GetCurrentUserId());

            await _unitOfWork.CartItemRepository.AddAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Game added to cart successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding game {GameId} to cart for user {UserId}",
                gameId, _currentUserHelper.GetCurrentUserId());
            return Result.Error("Unexpected error occurred");
        }
    }

    // ---------------------- Remove ----------------------
    public async Task<Result> RemoveFromCartAsync(int gameId)
    {
        try
        {
            var (validation, player) = await GetValidatedPlayerAsync();
            if (!validation.IsSuccess || player == null)
                return Result.Error(validation.Message ?? "Error removing from cart");

            var cart = player.Cart;
            if (cart == null) return Result.Error("Cart not found");

            if (!await _unitOfWork.GameRepository.AnyAsync(x => x.Id == gameId))
                return Result.Error("Game not found");

            var cartItem = await _unitOfWork.CartItemRepository
                .GetFirstOrDefaultAsync(x => x.CartId == cart.Id && x.GameId == gameId);

            if (cartItem == null)
                return Result.Error("Game not in cart");

            _unitOfWork.CartItemRepository.Delete(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Game removed from cart successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error removing game {GameId} from cart for user {UserId}",
                gameId, _currentUserHelper.GetCurrentUserId());
            return Result.Error("Unexpected error occurred");
        }
    }

    // ---------------------- Clear ----------------------
    public async Task<Result> ClearCartAsync()
    {
        try
        {
            var (validation, player) = await GetValidatedPlayerAsync();
            if (!validation.IsSuccess || player == null)
                return Result.Error(validation.Message ?? "Error clearing cart");

            var cart = player.Cart;
            if (cart == null) return Result.Error("Cart not found");

            var cartItems = await _unitOfWork.CartItemRepository.FindAsync(x => x.CartId == cart.Id);

            if (!cartItems.Any())
                return Result.Success("Cart is already empty"); 

            _unitOfWork.CartItemRepository.DeleteRange(cartItems);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Cart cleared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error clearing cart for user {UserId}",
                _currentUserHelper.GetCurrentUserId());
            return Result.Error("Unexpected error occurred");
        }
    }

    // ---------------------- Get Cart ----------------------
    public async Task<Result<CartResponse>> GetCurrentCartAsync(BasePaginationFilter filter)
    {
        try
        {
            var (validation, player) = await GetValidatedPlayerAsync();
            if (!validation.IsSuccess || player == null)
                return Result<CartResponse>.Error(validation.Message ?? "Error getting cart");

            var cart = player.Cart;

            if (cart == null)
            {
                // create new cart safely
                cart = new Cart { PlayerId = player.Id };
                cart.InitializeAudit(_currentUserHelper.GetCurrentUserId());

                await _unitOfWork.CartRepository.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            var (cartItems, totalItems) = await _unitOfWork.CartItemRepository.GetPagedAsync(
                filter.Page,
                filter.PageSize,
                x => x.CartId == cart.Id,
                x => x.CreatedAt!,
                false,//sort by created at
                x => x.Game
            );

            var totalPrice = await _unitOfWork.CartItemRepository.GetQueryable()
                .Where(x => x.CartId == cart.Id)
                .SumAsync(x => x.Game.Price);

            // Use AutoMapper profile for Cart -> CartResponse and items
            var response = _mapper.Map<CartResponse>(cart);
            // Manual business fields computed from repositories, not mapper
            response.TotalItems = totalItems;
            response.TotalPrice = totalPrice;
            response.CartItems = _mapper.Map<List<CartItemResponse>>(cartItems);

            return Result<CartResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting cart for user {UserId}",
                _currentUserHelper.GetCurrentUserId());
            return Result<CartResponse>.Error("Unexpected error occurred");
        }
    }

    public async Task<bool> IsInCurrentCartAsync(int gameId)
    {
        var (validation, player) = await GetValidatedPlayerAsync();
        if (!validation.IsSuccess || player?.Cart == null) return false;
        return await _unitOfWork.CartItemRepository.AnyAsync(x => x.CartId == player.Cart.Id && x.GameId == gameId);
    }

    public async Task<Result<List<BLL.DTOs.Cart.CartItemResponse>>> GetAllCurrentCartItemsAsync()
    {
        try
        {
            var (validation, player) = await GetValidatedPlayerAsync();
            if (!validation.IsSuccess || player?.Cart == null)
                return Result<List<BLL.DTOs.Cart.CartItemResponse>>.Error(validation.Message ?? "Cart not found");

            var cartItems = await _unitOfWork.CartItemRepository.FindAsync(x => x.CartId == player.Cart.Id, x => x.Game);
            var items = cartItems.Select(ci => new BLL.DTOs.Cart.CartItemResponse
            {
                Id = ci.Id,
                GameId = ci.GameId,
                GameTitle = ci.Game.Title,
                GameImagePath = ci.Game.ImagePath,
                Price = ci.Game.Price,
                CreatedAt = ci.CreatedAt
            }).ToList();
            return Result<List<BLL.DTOs.Cart.CartItemResponse>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all current cart items for user {UserId}", _currentUserHelper.GetCurrentUserId());
            return Result<List<BLL.DTOs.Cart.CartItemResponse>>.Error("Unexpected error occurred");
        }
    }
}