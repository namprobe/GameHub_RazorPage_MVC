using GameHub.DAL.Interfaces;
using GameHub.DAL.Entities;
using GameHub.DAL.Context;

namespace GameHub.DAL.Implements;

public class GameRegistrationDetailRepository : GenericRepository<GameRegistrationDetail>, IGameRegistrationDetailRepository
{
    public GameRegistrationDetailRepository(GameHubContext context) : base(context)
    {
        
    }
}