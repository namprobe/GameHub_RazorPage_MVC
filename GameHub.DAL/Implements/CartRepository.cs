using GameHub.DAL.Context;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;

namespace GameHub.DAL.Implements;

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    public CartRepository(GameHubContext context) : base(context)
    {
    }
}