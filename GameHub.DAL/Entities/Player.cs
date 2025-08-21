using System;
using System.Collections.Generic;
using GameHub.DAL.Common;

namespace GameHub.DAL.Entities;

public partial class Player : BaseEntity
{
    public int? UserId { get; set; }

    public string Username { get; set; } = null!;
    public string? AvatarPath { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual User? User { get; set; }
    public virtual ICollection<GameRegistration> GameRegistrations { get; set; } = new List<GameRegistration>();
    public virtual Cart Cart { get; set; } = null!;
}
