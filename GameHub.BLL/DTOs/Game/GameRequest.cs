using System.ComponentModel.DataAnnotations;
using GameHub.BLL.Validations;

namespace GameHub.BLL.DTOs.Game;

public class GameRequest
{
    [Required(ErrorMessage = "Title is required")]
    [GameTitleValidation]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999.99, ErrorMessage = "Price must be between $0.01 and $999.99")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Release Date is required")]
    [PastDateValidation]
    public DateOnly? ReleaseDate { get; set; }

    [Required(ErrorMessage = "Developer is required")]
    public int? DeveloperId { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public int? CategoryId { get; set; }
}