using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GameHub.BLL.Helpers;

public class FileUploadHelper
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<FileUploadHelper> _logger;
    private readonly int _maxFileSize = 10 * 1024 * 1024;
    private readonly string _baseUrl;

    public FileUploadHelper(IWebHostEnvironment webHostEnvironment, ILogger<FileUploadHelper> logger, IConfiguration configuration)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
        _baseUrl = configuration["BaseUrl"];
    }

    public async Task<string?> UploadFileAsync(IFormFile file, string? folder = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return null; // ko co file
            }

            if (file.Length > _maxFileSize)
            {
                throw new ArgumentException("File size exceeds the maximum allowed size");
            }

            //Tạo tên file duy nhất
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folder ?? "default");

            //Tạo thư mục nếu không tồn tại
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            //Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //Trả về đường dẫn file
            return $"/uploads/{folder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return null;
        }

    }

    public string? GetFileUrl(string relativeFilePath)
    {
        if (string.IsNullOrEmpty(relativeFilePath))
        {
            return null;
        }
        return $"{_baseUrl}/{relativeFilePath.TrimStart('/')}";
    }

    public bool DeleteFile(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false; //ko co file
            }

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));

            if (!File.Exists(fullPath))
            {
                return false;
            }

            File.Delete(fullPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return false;
        }

    }
}