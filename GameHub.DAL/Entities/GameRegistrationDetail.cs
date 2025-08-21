using GameHub.DAL.Common;

namespace GameHub.DAL.Entities;

public class GameRegistrationDetail : BaseEntity
{
    public int GameRegistrationId { get; set; }

    public int GameId { get; set; }

    public decimal Price { get; set; }

    public virtual GameRegistration GameRegistration { get; set; } = null!;

    public virtual Game Game { get; set; } = null!;
}