using GameHub.DAL.Context;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;

namespace GameHub.DAL.Implements;
public class DeveloperRepository : GenericRepository<Developer>, IDeveloperRepository
{
    public DeveloperRepository(GameHubContext context) : base(context)
    {
    }
}