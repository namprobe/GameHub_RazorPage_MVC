using System.ComponentModel.DataAnnotations;

namespace GameHub.BLL.DTOs.GameRegistration;

public class GameRegistrationRequest
{
    //allow null if user want to register multiple games (will get from cartItem list in database)
    public int? GameId { get; set; }
    public string? PaymentMethod { get; set; }
}