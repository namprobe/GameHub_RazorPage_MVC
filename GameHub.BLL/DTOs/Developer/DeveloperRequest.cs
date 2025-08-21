using System.ComponentModel.DataAnnotations;

namespace GameHub.BLL.DTOs.Developer;

public class DeveloperRequest
{
    [Required(ErrorMessage = "Developer name is required")]
    public string DeveloperName { get; set; } = string.Empty;
    
    public string? Website { get; set; }
}
