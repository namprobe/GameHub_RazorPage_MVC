namespace GameHub.BLL.DTOs.Payment;

public class PaymentResponse
{
    public int Id { get; set; }
    public int GameRegistrationId { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? CreatedAt { get; set; }
}