using GameHub.BLL.Models;

namespace GameHub.BLL.DTOs.GameRegistration;

public class GameRegistrationFilter : BasePaginationFilter
{
    public int? PlayerId { get; set; }
    public string? PlayerUserName { get; set; }
    public string? PlayerEmail { get; set; }
    public string? GameTitle { get; set; }
    public int? GameCategoryId { get; set; }
    public int? DeveloperId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

}