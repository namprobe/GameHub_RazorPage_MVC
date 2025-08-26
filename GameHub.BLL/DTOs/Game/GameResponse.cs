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
    public DateTime? CreatedAt { get; set; }
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public int RegistrationCount { get; set; } = 0;
    public int? CreatedBy { get; set; }

    public DeveloperItem? Developer { get; set; }

    public GameCategoryItem? Category { get; set; }
}