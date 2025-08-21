using GameHub.DAL.Common;
using GameHub.DAL.Enums;

namespace GameHub.DAL.Entities;
public class Payment : BaseEntity
{
    public int GameRegistrationId { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PayStatusEnum? PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public virtual GameRegistration GameRegistration { get; set; } = null!;
}