using GameHub.DAL.Context;
using GameHub.DAL.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameHub.DAL.Implements;
public class UnitOfWork : IUnitOfWork
{
    private readonly GameHubContext _context;
    private IDbContextTransaction? _transaction;
    private readonly IGameRepository _gameRepository;
    private readonly IGameCategoryRepository _gameCategoryRepository;
    private readonly IDeveloperRepository _developerRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGameRegistrationRepository _gameRegistrationRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IGameRegistrationDetailRepository _gameRegistrationDetailRepository;

    public UnitOfWork(
    GameHubContext context, IGameRepository gameRepository, 
    IGameCategoryRepository gameCategoryRepository, IDeveloperRepository developerRepository, 
    IPlayerRepository playerRepository, IUserRepository userRepository, 
    IGameRegistrationRepository gameRegistrationRepository, ICartRepository cartRepository, 
    ICartItemRepository cartItemRepository, IPaymentRepository paymentRepository,
    IGameRegistrationDetailRepository gameRegistrationDetailRepository)
    {
        _context = context;
        _gameRepository = gameRepository;
        _gameCategoryRepository = gameCategoryRepository;
        _developerRepository = developerRepository;
        _playerRepository = playerRepository;
        _userRepository = userRepository;
        _gameRegistrationRepository = gameRegistrationRepository;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _paymentRepository = paymentRepository;
        _gameRegistrationDetailRepository = gameRegistrationDetailRepository;
    }

    public IGameRepository GameRepository => _gameRepository;
    public IGameCategoryRepository GameCategoryRepository => _gameCategoryRepository;
    public IDeveloperRepository DeveloperRepository => _developerRepository;
    public IPlayerRepository PlayerRepository => _playerRepository;
    public IUserRepository UserRepository => _userRepository;

    public IGameRegistrationRepository GameRegistrationRepository => _gameRegistrationRepository;

    public ICartRepository CartRepository => _cartRepository;

    public ICartItemRepository CartItemRepository => _cartItemRepository;

    public IPaymentRepository PaymentRepository => _paymentRepository;

    public IGameRegistrationDetailRepository GameRegistrationDetailRepository => _gameRegistrationDetailRepository;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch (Exception)
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
}