using System.Linq.Expressions;
using GameHub.BLL.DTOs.GameRegistration;
using GameHub.BLL.Helpers;
using GameHub.DAL.Entities;

namespace GameHub.BLL.QueryBuilders;

public class GameRegistrationQueryBuilder : BaseQueryBuilder<GameRegistration, GameRegistrationFilter>
{
    /// <summary>
    /// Định nghĩa các properties để global search (filter.Search)
    /// </summary>
    protected override IEnumerable<Expression<Func<GameRegistration, string>>> GetSearchProperties()
    {
        return new List<Expression<Func<GameRegistration, string>>>
        {
            x => x.Player.Username,
            x => x.Player.User!.Email,
            x => x.GameRegistrationDetails.FirstOrDefault()!.Game.Title,
            x => x.GameRegistrationDetails.FirstOrDefault()!.Game.Category!.CategoryName,
            x => x.GameRegistrationDetails.FirstOrDefault()!.Game.Developer!.DeveloperName,
        };
    }
    
    /// <summary>
    /// Custom filter logic cho GameRegistration
    /// </summary>
    protected override Expression<Func<GameRegistration, bool>> BuildCustomPredicate(GameRegistrationFilter filter)
    {
        var predicate = PredicateBuilder.True<GameRegistration>();
        
        // Filter theo PlayerId (for admin to filter by specific player)
        if (filter.PlayerId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.PlayerId == filter.PlayerId.Value);
        }
        
        // Filter theo PlayerUserName
        if (!string.IsNullOrWhiteSpace(filter.PlayerUserName))
        {
            predicate = predicate.CombineAnd(x => x.Player.Username.Contains(filter.PlayerUserName));
        }
        
        // Filter theo PlayerEmail
        if (!string.IsNullOrWhiteSpace(filter.PlayerEmail))
        {
            predicate = predicate.CombineAnd(x => x.Player.User!.Email.Contains(filter.PlayerEmail));
        }
        
        // Filter theo GameTitle (search in registration details)
        if (!string.IsNullOrWhiteSpace(filter.GameTitle))
        {
            predicate = predicate.CombineAnd(x => x.GameRegistrationDetails.Any(d => d.Game.Title.Contains(filter.GameTitle)));
        }
        
        // Filter theo GameCategoryId
        if (filter.GameCategoryId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.GameRegistrationDetails.Any(d => d.Game.CategoryId == filter.GameCategoryId.Value));
        }
        
        // Filter theo DeveloperId
        if (filter.DeveloperId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.GameRegistrationDetails.Any(d => d.Game.DeveloperId == filter.DeveloperId.Value));
        }
        
        // Filter theo date range
        if (filter.StartDate.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.RegistrationDate >= filter.StartDate.Value);
        }
        
        if (filter.EndDate.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.RegistrationDate <= filter.EndDate.Value.AddDays(1));
        }
        
        return predicate;
    }
}
