using GameHub.DAL.Common;

namespace GameHub.DAL.Entities;
public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public int GameId { get; set; }
    public virtual Cart Cart { get; set; } = null!;
    public virtual Game Game { get; set; } = null!;
}