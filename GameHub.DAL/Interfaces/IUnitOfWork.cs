namespace GameHub.DAL.Interfaces;
public interface IUnitOfWork
{
    IGameRepository GameRepository { get; }
    IGameCategoryRepository GameCategoryRepository { get; }
    IDeveloperRepository DeveloperRepository { get; }
    IPlayerRepository PlayerRepository { get; }
    IUserRepository UserRepository { get; }
    IGameRegistrationRepository GameRegistrationRepository { get; }
    IGameRegistrationDetailRepository GameRegistrationDetailRepository { get; }
    ICartRepository CartRepository { get; }
    ICartItemRepository CartItemRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    void Dispose();
}