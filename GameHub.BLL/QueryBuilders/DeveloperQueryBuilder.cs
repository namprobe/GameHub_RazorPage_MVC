using System.Linq.Expressions;
using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Helpers;
using GameHub.DAL.Entities;

namespace GameHub.BLL.QueryBuilders;

public class DeveloperQueryBuilder : BaseQueryBuilder<Developer, DeveloperFilter>
{
    /// <summary>
    /// Định nghĩa các properties để global search (filter.Search)
    /// </summary>
    protected override IEnumerable<Expression<Func<Developer, string>>> GetSearchProperties()
    {
        return new List<Expression<Func<Developer, string>>>
        {
            x => x.DeveloperName,
            x => x.Website ?? string.Empty,
        };
    }
    
    /// <summary>
    /// Custom filter logic cho Developer
    /// </summary>
    protected override Expression<Func<Developer, bool>> BuildCustomPredicate(DeveloperFilter filter)
    {
        var predicate = PredicateBuilder.True<Developer>();
        
        // Filter cụ thể theo DeveloperName
        if (!string.IsNullOrWhiteSpace(filter.DeveloperName))
        {
            predicate = predicate.CombineAnd(x => x.DeveloperName.Contains(filter.DeveloperName));
        }
        
        // Filter theo Website
        if (!string.IsNullOrWhiteSpace(filter.Website))
        {
            predicate = predicate.CombineAnd(x => x.Website != null && x.Website.Contains(filter.Website));
        }
        
        return predicate;
    }
}
