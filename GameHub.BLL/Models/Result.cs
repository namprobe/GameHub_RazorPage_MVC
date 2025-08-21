namespace GameHub.BLL.Models;
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static Result<T> Success(T data, string message = "Success")
    {
        return new Result<T> { IsSuccess = true, Message = message, Data = data };
    }

    public static Result<T> Error(string message, List<string>? errors = null)
    {
        return new Result<T> { IsSuccess = false, Message = message, Errors = errors };
    }

    public static Result<T> Error(string message)
    {
        return new Result<T> { IsSuccess = false, Message = message };
    }
}

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static Result Success(string message = "Success")
    {
        return new Result { IsSuccess = true, Message = message };
    }

    public static Result Error(string message, List<string>? errors = null)
    {
        return new Result { IsSuccess = false, Message = message, Errors = errors };
    }

    public static Result Error(string message)
    {
        return new Result { IsSuccess = false, Message = message };
    }
}