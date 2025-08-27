using GameHub.BLL.DTOs.Payment;

namespace GameHub.BLL.DTOs.GameRegistration;

public class GameRegistrationResponse
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public DateTime RegistrationDate { get; set; }
    public decimal Price { get; set; }
    public DateTime? CreatedAt { get; set; }
    public IEnumerable<GameRegistrationItem> GameRegistrationItems { get; set; } = new List<GameRegistrationItem>();
    public PaymentResponse? Payment { get; set; }
}