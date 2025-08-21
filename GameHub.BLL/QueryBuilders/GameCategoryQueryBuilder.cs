using System.Linq.Expressions;
using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.Helpers;
using GameHub.BLL.Models;
using GameHub.DAL.Common;
using GameHub.DAL.Entities;

namespace GameHub.BLL.QueryBuilders;

public class GameCategoryQueryBuilder : BaseQueryBuilder<GameCategory, GameCategoryFilter>
{
    // Định nghĩa các search properties
    protected override IEnumerable<Expression<Func<GameCategory, string>>> GetSearchProperties()
    {
        return new List<Expression<Func<GameCategory, string>>>
        {
            x => x.CategoryName,
            x => x.Description ?? string.Empty,
        };
    }

    //Custom filter logic
    protected override Expression<Func<GameCategory, bool>> BuildCustomPredicate(GameCategoryFilter filter)
    {
        var predicate = PredicateBuilder.True<GameCategory>();

        if (!string.IsNullOrWhiteSpace(filter.CategoryName))
        {
            predicate = predicate.CombineAnd(x => x.CategoryName.Contains(filter.CategoryName));
        }

        if (!string.IsNullOrWhiteSpace(filter.Description))
        {
            predicate = predicate.CombineAnd(x => x.Description.Contains(filter.Description));
        }

        return predicate;
    }

    protected override Expression<Func<GameCategory, bool>> BuildSearchPredicate(GameCategoryFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.Search))
            return PredicateBuilder.True<GameCategory>();
        
        //Global search
        return QueryBuilderHelper.BuildSearchPredicate<GameCategory>(
            filter.Search,
            GetSearchProperties().ToArray()
        );
    }
}