namespace GameHub.BLL.DTOs.Cart;

public class CartResponse
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; } //total Items in cart, not total items in cartItems
    public List<CartItemResponse> CartItems { get; set; } = new();
}