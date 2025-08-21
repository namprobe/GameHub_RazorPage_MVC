using System;
using System.Collections.Generic;
using GameHub.DAL.Common;

namespace GameHub.DAL.Entities;

public partial class Developer : BaseEntity
{
    public string DeveloperName { get; set; } = null!;

    public string? Website { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
