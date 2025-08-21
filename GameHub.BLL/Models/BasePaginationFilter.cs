namespace GameHub.BLL.Models;
public class BasePaginationFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool? IsAscending { get; set; }
    public bool? IsActive { get; set; }
}