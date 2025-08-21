using GameHub.DAL.Context;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;

namespace GameHub.DAL.Implements;
public class GameCategoryRepository : GenericRepository<GameCategory>, IGameCategoryRepository
{
    public GameCategoryRepository(GameHubContext context) : base(context)
    {
    }
}