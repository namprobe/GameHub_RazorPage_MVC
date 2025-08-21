namespace GameHub.BLL.DTOs.Developer;

public class DeveloperResponse : DeveloperItem
{
    public string? Website { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
