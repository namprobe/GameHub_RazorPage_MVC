using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.DTOs.GameCategory;

namespace GameHub.BLL.DTOs.GameRegistration;

public class GameRegistrationItem
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string GameImagePath { get; set; } = string.Empty;
    public GameCategoryItem GameCategory { get; set; } = null!;
    public DeveloperItem Developer { get; set; } = null!;

    public decimal Price { get; set; }
    public DateTime? CreatedAt { get; set; }
}