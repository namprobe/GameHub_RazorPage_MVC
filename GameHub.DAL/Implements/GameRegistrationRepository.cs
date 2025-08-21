using GameHub.DAL.Context;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;

namespace GameHub.DAL.Implements;
public class GameRegistrationRepository : GenericRepository<GameRegistration>, IGameRegistrationRepository
{
    public GameRegistrationRepository(GameHubContext context) : base(context)
    {
    }
}