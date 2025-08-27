using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using GameHub.BLL.DTOs.GameRegistration;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.BLL.QueryBuilders;
using GameHub.DAL.Common;
using GameHub.DAL.Entities;
using GameHub.DAL.Enums;
using GameHub.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace GameHub.BLL.Implements;

public class GameRegistrationService : IGameRegistrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GameRegistrationService> _logger;
    private readonly CurrentUserHelper _currentUserHelper;
    private readonly IConfiguration _configuration;
    private readonly IVNPayHelper _vnPayHelper;
    public GameRegistrationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GameRegistrationService> logger, CurrentUserHelper currentUserHelper, IVNPayHelper vnPayHelper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserHelper = currentUserHelper;
        _vnPayHelper = vnPayHelper;
        _configuration = configuration;
    }

    // ---------------------- Currency helpers ----------------------
    private decimal GetUsdToVndRate()
    {
        // Read from config: Currency:UsdToVndRate, fallback 25000
        var rateStr = _configuration["Currency:UsdToVndRate"] ?? "25000"; 
        if (decimal.TryParse(rateStr, out var rate) && rate > 0) return rate;
        return 25000m; // sensible default
    }

    private decimal ConvertUsdToVnd(decimal amountUsd)
    {
        var vnd = amountUsd * GetUsdToVndRate();
        // VNPay requires integer VND (no decimals). Round to nearest VND.
        return Math.Round(vnd, 0, MidpointRounding.AwayFromZero);
    }

    private decimal ConvertVndToUsd(decimal amountVnd)
    {
        var rate = GetUsdToVndRate();
        if (rate <= 0) return amountVnd; 
        return Math.Round(amountVnd / rate, 2, MidpointRounding.AwayFromZero);
    }

    private async Task<Result<Player>> ValidateCurrentPlayerAsync()
    {
        if (!await _currentUserHelper.IsCurrentUserPlayerAsync())
            return Result<Player>.Error("Not authorized");

        var currentUserId = _currentUserHelper.GetCurrentUserId();
        var currentPlayer = await _unitOfWork.PlayerRepository
            .GetFirstOrDefaultAsync(x => x.UserId == currentUserId);

        return currentPlayer == null
            ? Result<Player>.Error("Player not found")
            : Result<Player>.Success(currentPlayer);
    }

    public async Task<Result<GameRegistrationResponse>> GetByIdAsync(int id)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateCurrentUserAsync();
            if (!success)
                return Result<GameRegistrationResponse>.Error(error?? "Not authorized");
            
            // Get game registration with basic details first
            var gameRegistration = await _unitOfWork.GameRegistrationRepository.GetFirstOrDefaultAsync(x => x.Id == id, 
                x => x.Player,
                x => x.Player.User,
                x => x.GameRegistrationDetails,
                x => x.Payment
            );
            
            if (gameRegistration == null)
                return Result<GameRegistrationResponse>.Error("Game registration not found");

            // Load nested Game data manually
            if (gameRegistration.GameRegistrationDetails?.Any() == true)
            {
                foreach (var detail in gameRegistration.GameRegistrationDetails)
                {
                    if (detail.GameId > 0)
                    {
                        detail.Game = await _unitOfWork.GameRepository.GetFirstOrDefaultAsync(
                            g => g.Id == detail.GameId,
                            g => g.Category,
                            g => g.Developer
                        );
                    }
                }
            }
            
            // Check authorization: Players can only view their own registrations
            var currentUser = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(x => x.Id == _currentUserHelper.GetCurrentUserId());
            if (currentUser?.Role == RoleEnum.Player)
            {
                var currentPlayer = await _unitOfWork.PlayerRepository.GetFirstOrDefaultAsync(x => x.UserId == currentUser.Id);
                if (currentPlayer?.Id != gameRegistration.PlayerId)
                    return Result<GameRegistrationResponse>.Error("Not authorized to view this registration");
            }
            
            var response = _mapper.Map<GameRegistrationResponse>(gameRegistration);
            return Result<GameRegistrationResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game registration");
            return Result<GameRegistrationResponse>.Error("An error occurred while getting game registration");
        }
    }

    public async Task<PaginationResult<GameRegistrationResponse>> GetPagedAsync(GameRegistrationFilter filter)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateCurrentUserAsync();
            if (!success)
                return PaginationResult<GameRegistrationResponse>.Error(error?? "Not authorized");
            
            var currentUser = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(x => x.Id == _currentUserHelper.GetCurrentUserId());
            if (currentUser == null)
                return PaginationResult<GameRegistrationResponse>.Error("User not found");
            
            // Set PlayerId filter for players (they can only see their own registrations)
            if (currentUser.Role == RoleEnum.Player)
            {
                var player = await _unitOfWork.PlayerRepository.GetFirstOrDefaultAsync(x => x.UserId == currentUser.Id);
                if (player == null)
                    return PaginationResult<GameRegistrationResponse>.Error("Player not found");
                filter.PlayerId = player.Id;
            }
            
            // Build query using GameRegistrationQueryBuilder
            var queryBuilder = new GameRegistrationQueryBuilder();
            var predicate = queryBuilder.BuildPredicate(filter);
            var orderBy = queryBuilder.BuildOrderBy(filter);
            
            // Get paged data with basic includes first
            var (items, totalCount) = await _unitOfWork.GameRegistrationRepository.GetPagedAsync(
                filter.Page,
                filter.PageSize,
                predicate,
                orderBy?? (o => o.RegistrationDate!),
                filter.IsAscending ?? false,//default newest first
                x => x.Player,
                x => x.Player.User!,
                x => x.GameRegistrationDetails,
                x => x.Payment
            );

            // Load nested data for Game, Category, and Developer manually
            foreach (var registration in items)
            {
                if (registration.GameRegistrationDetails?.Any() == true)
                {
                    foreach (var detail in registration.GameRegistrationDetails)
                    {
                        if (detail.GameId > 0)
                        {
                            detail.Game = await _unitOfWork.GameRepository.GetFirstOrDefaultAsync(
                                g => g.Id == detail.GameId,
                                g => g.Category,
                                g => g.Developer
                            );
                        }
                    }
                }
            }
            
            // Map to response
            var responses = _mapper.Map<IEnumerable<GameRegistrationResponse>>(items);
            
            return PaginationResult<GameRegistrationResponse>.Success(
                responses.ToList(), 
                filter.Page, 
                filter.PageSize, 
                totalCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged game registrations");
            return PaginationResult<GameRegistrationResponse>.Error("An error occurred while getting game registrations");
        }
    }

    public async Task<Result<string>> RegisterGameAsync(GameRegistrationRequest request)
    {
        try
        {
            var validationResult = await ValidateCurrentPlayerAsync();
            if (!validationResult.IsSuccess)
                return Result<string>.Error(validationResult.Message ?? "Error registering game");

            var player = validationResult.Data;
            //create game registration
            var gameRegistration = new GameRegistration
            {
                PlayerId = player!.Id,
                RegistrationDate = DateTime.Now
            };
            //create game registration details
            var gameRegistrationDetails = new List<GameRegistrationDetail>();

            //initialize cart items
            IEnumerable<CartItem> cartItems = new List<CartItem>();
            
            if (request.GameId != null) //register single game immediately
            {
                var game = await _unitOfWork.GameRepository.GetFirstOrDefaultAsync(x => x.Id == request.GameId);
                if (game == null)
                    return Result<string>.Error("Game not found");

                //check if game is already registered
                var existingRegistrationDetail = await _unitOfWork.GameRegistrationDetailRepository.GetFirstOrDefaultAsync(x => x.GameRegistration.PlayerId == player!.Id && x.GameId == game.Id, x => x.GameRegistration);
                if (existingRegistrationDetail != null && existingRegistrationDetail.GameRegistration != null)
                {
                    var existingPayment = await _unitOfWork.PaymentRepository.GetFirstOrDefaultAsync(x => x.GameRegistrationId == existingRegistrationDetail.GameRegistration.Id);
                    if (existingPayment != null && (existingPayment.PaymentStatus == PayStatusEnum.Success || existingPayment.PaymentStatus == PayStatusEnum.Pending))
                        return Result<string>.Error("Game already registered");
                }

                var singleGameRegistrationDetail = new GameRegistrationDetail
                {
                    GameRegistration = gameRegistration,
                    GameId = game.Id,
                    Price = game.Price,
                    IsActive = false,//not active until payment is successful
                };
                singleGameRegistrationDetail.InitializeAudit(player?.UserId);
                gameRegistrationDetails.Add(singleGameRegistrationDetail);
                gameRegistration.PurchasePrice = singleGameRegistrationDetail.Price;
            }
            else //register multiple games from cart
            {
                var cart = await _unitOfWork.CartRepository.GetFirstOrDefaultAsync(x => x.PlayerId == player!.Id);
                if (cart == null)
                    return Result<string>.Error("Cart not found");
                cartItems = await _unitOfWork.CartItemRepository.FindAsync(x => x.CartId == cart.Id, x => x.Game);
                if (!cartItems.Any())
                    return Result<string>.Error("Cart items not found");
                decimal totalPrice = 0m;
                
                foreach (var cartItem in cartItems)
                {
                    //check if game is already registered
                    var existingRegistrationDetail = await _unitOfWork.GameRegistrationDetailRepository.GetFirstOrDefaultAsync(x => x.GameRegistration.PlayerId == player!.Id && x.GameId == cartItem.Game.Id, x => x.GameRegistration);
                    if (existingRegistrationDetail != null && existingRegistrationDetail.GameRegistration != null)
                    {
                        var existingPayment = await _unitOfWork.PaymentRepository.GetFirstOrDefaultAsync(x => x.GameRegistrationId == existingRegistrationDetail.GameRegistration.Id);
                        if (existingPayment != null && (existingPayment.PaymentStatus == PayStatusEnum.Success || existingPayment.PaymentStatus == PayStatusEnum.Pending))
                            return Result<string>.Error("Some games in cart are already registered");
                    }
                    var gameRegistrationDetail = new GameRegistrationDetail
                    {
                        GameRegistration = gameRegistration,
                        GameId = cartItem.Game.Id,
                        Price = cartItem.Game.Price,
                        IsActive = true,
                    };
                    gameRegistrationDetail.InitializeAudit(player?.UserId);
                    gameRegistrationDetails.Add(gameRegistrationDetail);
                    totalPrice += cartItem.Game.Price;
                }
                gameRegistration.PurchasePrice = totalPrice;
            }
            gameRegistration.InitializeAudit(player?.UserId);
            
            //Initialize payment
            var payment = new Payment
            {
                GameRegistration = gameRegistration,
                PaymentMethod = request.PaymentMethod,
                Amount = gameRegistration.PurchasePrice,
                //PaymentDate = DateTime.Now, // Set payment date to avoid DateTime.MinValue
                PaymentStatus = PayStatusEnum.Pending,
                IsActive = true
            };
            payment.InitializeAudit(player?.UserId);
            //transaction handling
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.GameRegistrationRepository.AddAsync(gameRegistration);
                await _unitOfWork.GameRegistrationDetailRepository.AddRangeAsync(gameRegistrationDetails);
                await _unitOfWork.PaymentRepository.AddAsync(payment);
                //clear cart items
                _unitOfWork.CartItemRepository.DeleteRange(cartItems);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            //create payment url (VNPay requires VND)
            var vndAmount = ConvertUsdToVnd(payment.Amount);
            var paymentUrl = _vnPayHelper.CreatePaymentUrl(gameRegistration.Id, vndAmount);
            return Result<string>.Success(paymentUrl, "Payment URL created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering game");
            return Result<string>.Error("An error occurred while registering game");
        }
    }

    // public async Task<Result<string>> UpdateRegistrationAtferCallBackPayment(IQueryCollection queryParams)
    // {
    //     try
    //     {
    //         var vnPayResult = _vnPayHelper.VerifyPaymentResponse(queryParams);
    //     }
    // }

    public async Task<Result> ToggleStatusAsync(int id)
    {
        try
        {
            var (success, error) = await _currentUserHelper.ValidateAdminUserAsync();
            if (!success)
                return Result.Error(error?? "Not authorized");
            var gameRegistration = await _unitOfWork.GameRegistrationRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (gameRegistration == null)
                return Result.Error("Game registration not found");
            gameRegistration.IsActive = !gameRegistration.IsActive;
            gameRegistration.UpdateAudit(_currentUserHelper.GetCurrentUserId());
            _unitOfWork.GameRegistrationRepository.Update(gameRegistration);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success("Game registration status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating game registration");
            return Result.Error("An error occurred while updating game registration");
        }
    }

    public async Task<Result> HandleVnPayReturnAsync(IQueryCollection queryParams)
    {
        try
        {
            // Verify VNPay signature first
            var isValid = _vnPayHelper.VerifyPaymentResponse(queryParams);
            if (!isValid)
                return Result.Error("Invalid VNPay signature");

            // Extract fields
            var txnRef = queryParams["vnp_TxnRef"].ToString(); // This is GameRegistration.Id
            var transactionNo = queryParams["vnp_TransactionNo"].ToString();
            var responseCode = queryParams["vnp_ResponseCode"].ToString();
            var transactionStatus = queryParams["vnp_TransactionStatus"].ToString();
            var amountStr = queryParams["vnp_Amount"].ToString();

            if (!int.TryParse(txnRef, out var registrationId))
                return Result.Error("Invalid transaction reference");

            // Load registration and its payment (include Player to get UserId)
            var registration = await _unitOfWork.GameRegistrationRepository.GetFirstOrDefaultAsync(
                x => x.Id == registrationId,
                x => x.Player
            );
            if (registration == null)
                return Result.Error("Registration not found");

            var payment = await _unitOfWork.PaymentRepository.GetFirstOrDefaultAsync(x => x.GameRegistrationId == registration.Id);
            if (payment == null)
                return Result.Error("Payment not found");

            // Get the user ID from the registration's player for audit purposes
            var userIdForAudit = registration.Player?.UserId;

            // Optional amount validation (VNPay amount = original * 100)
            if (decimal.TryParse(amountStr, out var vnAmount100))
            {
                var expected100 = payment.Amount * 100m;
                // if (vnAmount100 != expected100) return Result.Error("Amount mismatch");
            }

            var isSuccess = responseCode == "00" && transactionStatus == "00";

            // Apply transactional updates
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                payment.TransactionId = transactionNo;
                payment.PaymentStatus = isSuccess ? PayStatusEnum.Success : PayStatusEnum.Failed;
                payment.PaymentDate = DateTime.Now;
                payment.UpdateAudit(userIdForAudit);
                _unitOfWork.PaymentRepository.Update(payment);

                if (isSuccess)
                {
                    registration.IsActive = true;
                    registration.UpdateAudit(userIdForAudit);
                    _unitOfWork.GameRegistrationRepository.Update(registration);

                    // Increment game registration counts
                    var details = await _unitOfWork.GameRegistrationDetailRepository
                        .FindAsync(d => d.GameRegistrationId == registration.Id, d => d.Game);
                    foreach (var d in details)
                    {
                        if (d.Game != null)
                        {
                            d.Game.RegistrationCount += 1;
                            _unitOfWork.GameRepository.Update(d.Game);
                        }
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            return Result.Success(isSuccess ? "Payment success" : "Payment failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling VNPay return");
            return Result.Error("An error occurred while processing payment result");
        }
    }
}