namespace GameHub.BLL.DTOs.Cart;

public class CartResponse
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public List<CartItemResponse> CartItems { get; set; } = new();
}