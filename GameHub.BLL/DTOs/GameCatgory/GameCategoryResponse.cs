namespace GameHub.BLL.DTOs.GameCategory;
public class GameCategoryResponse : GameCategoryItem
{
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}