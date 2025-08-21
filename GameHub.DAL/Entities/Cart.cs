using GameHub.DAL.Common;

namespace GameHub.DAL.Entities;
public class Cart : BaseEntity
{
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}