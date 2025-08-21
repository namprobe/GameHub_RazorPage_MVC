using GameHub.DAL.Context;
using GameHub.DAL.Entities;
using GameHub.DAL.Interfaces;

namespace GameHub.DAL.Implements;
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(GameHubContext context) : base(context)
    {
    }
}