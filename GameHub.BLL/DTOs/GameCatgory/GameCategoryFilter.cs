using GameHub.BLL.Models;

namespace GameHub.BLL.DTOs.GameCategory;

public class GameCategoryFilter : BasePaginationFilter
{
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
}