using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using GameHub.BLL.DTOs.Cart;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameHub.BLL.Implements;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CurrentUserHelper _currentUserHelper;
    private readonly ILogger<CartService> _logger;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper, CurrentUserHelper currentUserHelper, ILogger<CartService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserHelper = currentUserHelper;
        _logger = logger;
    }

    public async Task<Result> AddToCartAsync(CartItemRequest request)
    {
        try
        {
            var validateResult = await ValidateCurrentPlayerAsync();
            if (!validateResult.IsSuccess)
            {
                return Result.Error(validateResult.Message ?? "Error adding to cart");
            }

            var currentPlayer = validateResult.Data;

            var cart = currentPlayer.Cart;
            if (cart == null)
            {
                return Result.Error("Cart not found");
            }

            var isGameExist = await _unitOfWork.GameRepository.AnyAsync(x => x.Id == request.GameId);
            if (!isGameExist)
            {
                return Result.Error("Game not found");
            }

            var isAllowAdd = await _unitOfWork.CartItemRepository.AnyAsync(x => x.CartId == cart.Id && x.GameId == request.GameId);
            if (isAllowAdd)
            {
                return Result.Error("Game already in cart");
            }
            var cartItem = _mapper.Map<CartItem>(request);
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.CartItemRepository.AddAsync(cartItem);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch 
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return Result.Success("Game added to cart successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to cart");
            return Result.Error("Error adding to cart");
        }
    }

    public Task<Result> ClearCartAsync()
    {
        throw new NotImplementedException();
    }

    private async Task<Result<Player>> ValidateCurrentPlayerAsync()
    {
        var isValid = await _currentUserHelper.IsCurrentUserPlayerAsync();
        if (!isValid)
        {
            return Result<Player>.Error("Not authorized");
        }

        var currentUserId = _currentUserHelper.GetCurrentUserId();

        var currentPlayer = await _unitOfWork.PlayerRepository.GetFirstOrDefaultAsync(x => x.UserId == currentUserId, x => x.Cart);
        if (currentPlayer == null)
        {
            return Result<Player>.Error("Player not found");
        }

        return Result<Player>.Success(currentPlayer);
    }

    public async Task<Result<CartResponse>> GetCurrentCartAsync()
    {
        try
        {
            var validateResult = await ValidateCurrentPlayerAsync();
            if (!validateResult.IsSuccess)
            {
                return Result<CartResponse>.Error(validateResult.Message ?? "Error getting current cart");
            }

            var currentPlayer = validateResult.Data;

            var cart = currentPlayer.Cart;

            //If cart is null, create a new cart
            if (cart == null)
            {
                cart = new Cart
                {
                    PlayerId = currentPlayer.Id,
                };
                try
                {
                    await _unitOfWork.CartRepository.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating cart");
                    throw;
                }
            }

            var cartItems = await _unitOfWork.CartItemRepository.GetAllAsync(x => x.CartId == cart.Id, x => x.Game);
            var cartResponse = _mapper.Map<CartResponse>(cart);
            cartResponse.CartItems = _mapper.Map<List<CartItemResponse>>(cartItems);

            return Result<CartResponse>.Success(cartResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current cart");
            return Result<CartResponse>.Error("Error getting current cart");
        }
    }

    public Task<Result> RemoveFromCartAsync(int gameId)
    {
        throw new NotImplementedException();
    }
}