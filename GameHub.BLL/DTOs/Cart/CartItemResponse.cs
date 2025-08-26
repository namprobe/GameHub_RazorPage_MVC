namespace GameHub.BLL.DTOs.Cart;

public class CartItemResponse
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string GameTitle { get; set; } = null!;
    public string GameImagePath { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime? CreatedAt { get; set; }
}