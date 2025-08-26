namespace GameHub.BLL.DTOs.Cart;

public class CartItemResponse
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public DateTime? CreatedAt { get; set; }
}