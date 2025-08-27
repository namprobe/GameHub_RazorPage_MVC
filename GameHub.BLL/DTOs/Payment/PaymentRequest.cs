using GameHub.DAL.Enums;

namespace GameHub.BLL.DTOs.Payment;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }

}