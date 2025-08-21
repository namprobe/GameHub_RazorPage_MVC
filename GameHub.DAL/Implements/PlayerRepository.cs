using GameHub.DAL.Context;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;

namespace GameHub.DAL.Implements;
public class PlayerRepository : GenericRepository<Player>, IPlayerRepository
{
    public PlayerRepository(GameHubContext context) : base(context)
    {
    }
}