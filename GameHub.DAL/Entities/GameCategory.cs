using System;
using System.Collections.Generic;
using GameHub.DAL.Common;

namespace GameHub.DAL.Entities;

public partial class GameCategory : BaseEntity
{

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
