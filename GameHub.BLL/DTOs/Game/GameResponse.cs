using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.DTOs.GameCategory;

namespace GameHub.BLL.DTOs.Game;

public class GameResponse
{
    public int Id { get; set; }
    
    public string Title { get; set; } = null!;

    public decimal Price { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public bool IsActive { get; set; }
    
    public int PlayerCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    public DeveloperItem? Developer { get; set; }

    public GameCategoryItem? Category { get; set; }
}