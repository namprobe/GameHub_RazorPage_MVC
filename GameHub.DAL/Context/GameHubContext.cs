using GameHub.DAL.Entities;
using GameHub.DAL.Common;
using Microsoft.EntityFrameworkCore;

namespace GameHub.DAL.Context;

public partial class GameHubContext : DbContext
{
    public GameHubContext()
    {
    }

    public GameHubContext(DbContextOptions<GameHubContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Developer> Developers { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameCategory> GameCategories { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<GameRegistration> GameRegistrations { get; set; }
    public virtual DbSet<GameRegistrationDetail> GameRegistrationDetails { get; set; }
    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base Entity Configuration
        ConfigureBaseEntity(modelBuilder);
        modelBuilder.Entity<Developer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Develope__DE084CF10D25F437");

            entity.ToTable("Developer");

            entity.HasIndex(e => e.DeveloperName, "UQ__Develope__08E3F54D12E338F9").IsUnique();

            entity.Property(e => e.DeveloperName).HasMaxLength(200);
            entity.Property(e => e.Website).HasMaxLength(250);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Game__2AB897FD4A85B42A");

            entity.ToTable("Game");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Category).WithMany(p => p.Games)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Game__CategoryId__300424B4");

            entity.HasOne(d => d.Developer).WithMany(p => p.Games)
                .HasForeignKey(d => d.DeveloperId)
                .HasConstraintName("FK__Game__DeveloperI__2F10007B");
        });

        modelBuilder.Entity<GameCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GameCate__19093A0BBEA855B1");

            entity.ToTable("GameCategory");

            entity.HasIndex(e => e.CategoryName, "UQ__GameCate__8517B2E0CCCD0F67").IsUnique();

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Player__4A4E74C8E12D106F");

            entity.ToTable("Player");

            entity.HasIndex(e => e.UserId, "UQ__Player__1788CC4D25A05436").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Player__536C85E471E6807B").IsUnique();

            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.Player)
                .HasForeignKey<Player>(d => d.UserId)
                .HasConstraintName("FK__Player__UserId__34C8D9D1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__1788CC4CA7A739C2");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D105343A96CAA8").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.JoinDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
        });

        modelBuilder.Entity<GameRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GameRegi__6EF5881084F4A4F2");

            entity.ToTable("GameRegistration");

            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 2)");

            // Foreign key relationships
            entity.HasOne(d => d.Player).WithMany(p => p.GameRegistrations)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK__GameRegi__PlayerId");
        });

        modelBuilder.Entity<GameRegistrationDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GameRegiDetail__ID");

            entity.ToTable("GameRegistrationDetail");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            // Each game appears only once per registration
            entity.HasIndex(e => new { e.GameRegistrationId, e.GameId })
                .IsUnique()
                .HasDatabaseName("UQ__GameRegDetail__RegGame");

            entity.HasOne(d => d.GameRegistration).WithMany(p => p.GameRegistrationDetails)
                .HasForeignKey(d => d.GameRegistrationId)
                .HasConstraintName("FK__GameRegDetail__RegistrationId");

            entity.HasOne(d => d.Game).WithMany(p => p.GameRegistrationDetails)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK__GameRegDetail__GameId");
        });

        // Cart Configuration
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cart__51BCD79739D3C2B1");

            entity.ToTable("Cart");

            // One-to-One relationship: Player có 1 Cart
            entity.HasOne(d => d.Player).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.PlayerId)
                .HasConstraintName("FK__Cart__PlayerId");
        });

        // CartItem Configuration
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CartItem__488B0B2A4F4E4F4F");

            entity.ToTable("CartItem");

            // Composite unique index (một cart chỉ có thể có một game duy nhất)
            entity.HasIndex(e => new { e.CartId, e.GameId })
                .IsUnique()
                .HasDatabaseName("UQ__CartItem__CartGame");

            // Foreign key relationships
            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CartItem__CartId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Game).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK__CartItem__GameId");
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__9B556A58C7F7F7F7");

            entity.ToTable("Payment");

            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);

            // Foreign key relationship - One GameRegistration has One Payment
            entity.HasOne(d => d.GameRegistration).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.GameRegistrationId)
                .HasConstraintName("FK__Payment__GameRegistrationId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    
    /// <summary>
    /// Cấu hình BaseEntity cho tất cả entities
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder</param>
    private void ConfigureBaseEntity(ModelBuilder modelBuilder)
    {
        // Cấu hình chung cho tất cả BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
        {
            // Primary key
            modelBuilder.Entity(entityType.ClrType).HasKey("Id");
            
            // Default values for audit fields
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime?>("CreatedAt")
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
                
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime?>("UpdatedAt")
                .HasColumnType("datetime");
                
            modelBuilder.Entity(entityType.ClrType)
                .Property<bool>("IsActive")
                .HasDefaultValue(true);
        }
    }
}
