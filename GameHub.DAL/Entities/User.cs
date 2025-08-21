using System;
using System.Collections.Generic;
using GameHub.DAL.Common;
using GameHub.DAL.Enums;

namespace GameHub.DAL.Entities;

public partial class User : BaseEntity
{
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime JoinDate { get; set; }
    public RoleEnum Role { get; set; } = RoleEnum.Player;
    public virtual Player? Player { get; set; }
}
