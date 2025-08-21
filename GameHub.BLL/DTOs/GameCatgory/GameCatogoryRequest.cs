using System.ComponentModel.DataAnnotations;

namespace GameHub.BLL.DTOs.GameCategory;
public class GameCategoryRequest
{
    [Required(ErrorMessage = "Category name is required")]
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}