using System;
using System.Collections.Generic;
using GameHub.DAL.Common;

namespace GameHub.DAL.Entities;

public partial class Game : BaseEntity
{
    public string Title { get; set; } = null!;

    public decimal Price { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    public int RegistrationCount { get; set; } = 0;
    public int? DeveloperId { get; set; }

    public int? CategoryId { get; set; }

    public virtual GameCategory? Category { get; set; }

    public virtual Developer? Developer { get; set; }
    public virtual ICollection<GameRegistrationDetail> GameRegistrationDetails { get; set; } = new List<GameRegistrationDetail>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
