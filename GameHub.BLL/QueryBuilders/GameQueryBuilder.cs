using System.Linq.Expressions;
using GameHub.BLL.DTOs.Game;
using GameHub.BLL.Helpers;
using GameHub.DAL.Entities;

namespace GameHub.BLL.QueryBuilders;

public class GameQueryBuilder : BaseQueryBuilder<Game, GameFilter>
{
    /// <summary>
    /// Định nghĩa các properties để global search (filter.Search)
    /// </summary>
    protected override IEnumerable<Expression<Func<Game, string>>> GetSearchProperties()
    {
        return new List<Expression<Func<Game, string>>>
        {
            x => x.Title,
            x => x.Developer!.DeveloperName,
            x => x.Category!.CategoryName,
        };
    }
    
    /// <summary>
    /// Custom filter logic cho Game
    /// </summary>
    protected override Expression<Func<Game, bool>> BuildCustomPredicate(GameFilter filter)
    {
        var predicate = PredicateBuilder.True<Game>();
        
        // Filter cụ thể theo Title
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            predicate = predicate.CombineAnd(x => x.Title.Contains(filter.Title));
        }
        
        // Filter theo price range
        if (filter.MinPrice.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Price >= filter.MinPrice.Value);
        }
        
        if (filter.MaxPrice.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Price <= filter.MaxPrice.Value);
        }
        
        // Filter theo release date range
        if (filter.FromReleaseDate.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.ReleaseDate >= filter.FromReleaseDate.Value);
        }
        
        if (filter.ToReleaseDate.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.ReleaseDate <= filter.ToReleaseDate.Value);
        }
        
        // Filter theo DeveloperId
        if (filter.DeveloperId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.DeveloperId == filter.DeveloperId.Value);
        }
        // Multi DeveloperIds
        if (filter.DeveloperIds != null && filter.DeveloperIds.Any())
        {
            var devIds = filter.DeveloperIds.ToList();
            predicate = predicate.CombineAnd(x => x.DeveloperId.HasValue && devIds.Contains(x.DeveloperId.Value));
        }
        
        // Filter theo CategoryId
        if (filter.CategoryId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.CategoryId == filter.CategoryId.Value);
        }
        // Multi CategoryIds
        if (filter.CategoryIds != null && filter.CategoryIds.Any())
        {
            var catIds = filter.CategoryIds.ToList();
            predicate = predicate.CombineAnd(x => x.CategoryId.HasValue && catIds.Contains(x.CategoryId.Value));
        }
        // Exclude specific GameIds if provided
        if (filter.GameIds != null && filter.GameIds.Any())
        {
            var gids = filter.GameIds.ToList();
            predicate = predicate.CombineAnd(x => !gids.Contains(x.Id));
        }
        
        return predicate;
    }
}
