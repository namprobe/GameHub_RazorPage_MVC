using GameHub.BLL.Models;

namespace GameHub.BLL.DTOs.Developer;

public class DeveloperFilter : BasePaginationFilter
{
    public string? DeveloperName { get; set; }
    public string? Website { get; set; }
}
