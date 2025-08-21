using GameHub.DAL.Common;

namespace GameHub.DAL.Entities;
public class GameRegistration : BaseEntity
{
    
    public int PlayerId { get; set; }
    public DateTime RegistrationDate { get; set; }
    
    public decimal PurchasePrice { get; set; }
    
    // Navigation properties
    public virtual Player Player { get; set; } = null!;
    public virtual ICollection<GameRegistrationDetail> GameRegistrationDetails { get; set; } = new List<GameRegistrationDetail>();
}