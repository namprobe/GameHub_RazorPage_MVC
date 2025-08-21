using GameHub.BLL.Models;

namespace GameHub.BLL.DTOs.Game;

public class GameFilter : BasePaginationFilter
{
    public string? Title { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DateOnly? FromReleaseDate { get; set; }
    public DateOnly? ToReleaseDate { get; set; }
    public int? DeveloperId { get; set; }
    public int? CategoryId { get; set; }
}