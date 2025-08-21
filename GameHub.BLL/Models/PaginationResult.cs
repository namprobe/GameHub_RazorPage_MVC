namespace GameHub.BLL.Models;

public class PaginationResult<T>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public bool IsSuccess { get; set; } 
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static PaginationResult<T> Success(List<T> items, int pageNumber, int pageSize, int totalItems)
    {
        return new PaginationResult<T>
        {
            IsSuccess = true,
            CurrentPage = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public static PaginationResult<T> Error(string message, List<string>? errors = null)
    {
        return new PaginationResult<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors
        };
    }

    public static PaginationResult<T> Error(string message)
    {
        return new PaginationResult<T>
        {
            IsSuccess = false,
            Message = message
        };
    }
}