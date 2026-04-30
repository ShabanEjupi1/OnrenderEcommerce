using ProjectTemplate.Models;
using ProjectTemplate.Models.Ecommerce;
using Microsoft.EntityFrameworkCore;

namespace ProjectTemplate.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // -- Game / Learning -------------------------------------------------------
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<AnswerRecord> AnswerRecords => Set<AnswerRecord>();
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();

    // -- Legacy YourBrand ------------------------------------------------------
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<PosSystem> PosSystems => Set<PosSystem>();
    public DbSet<ProductItem> ProductItems => Set<ProductItem>();
    public DbSet<POSOrder> POSOrders => Set<POSOrder>();
    public DbSet<PosStaff> PosStaff => Set<PosStaff>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    // -- Ecommerce -------------------------------------------------------------
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // -- Game entities ------------------------------------------------------
        mb.Entity<Chapter>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Choices)
             .WithOne(x => x.Chapter)
             .HasForeignKey(x => x.ChapterId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<GameSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Answers)
             .WithOne(x => x.GameSession)
             .HasForeignKey(x => x.GameSessionId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.SessionKey);
        });

        mb.Entity<AnswerRecord>(e => e.HasKey(x => x.Id));
        mb.Entity<LeaderboardEntry>(e => e.HasKey(x => x.Id));

        // -- Legacy YourBrand ---------------------------------------------------
        mb.Entity<Business>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.PosSystems)
             .WithOne(x => x.Business)
             .HasForeignKey(x => x.BusinessId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        mb.Entity<PosSystem>(e => e.HasKey(x => x.Id));
        mb.Entity<ProductItem>(e => e.HasKey(x => x.Id));
        mb.Entity<POSOrder>(e => e.HasKey(x => x.Id));
        mb.Entity<PosStaff>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Business)
             .WithMany()
             .HasForeignKey(x => x.BusinessId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        mb.Entity<InventoryMovement>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.ProductItem)
             .WithMany()
             .HasForeignKey(x => x.ProductItemId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        mb.Entity<Vendor>(e => e.HasKey(x => x.Id));
        mb.Entity<Customer>(e => e.HasKey(x => x.Id));
        mb.Entity<PurchaseOrder>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Vendor)
             .WithMany()
             .HasForeignKey(x => x.VendorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // -- Ecommerce entities -------------------------------------------------
        mb.Entity<ProductCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(150);
        });

        mb.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Price).HasColumnType("decimal(10,2)");
            e.Property(x => x.SalePrice).HasColumnType("decimal(10,2)");
            e.Property(x => x.StockQuantity).HasColumnType("decimal(10,2)");
            e.HasOne(x => x.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
            e.HasIndex(x => x.Name);
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => x.IsActive);
        });

        mb.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OrderNumber).IsRequired().HasMaxLength(20);
            e.Property(x => x.TotalAmount).HasColumnType("decimal(10,2)");
            e.Property(x => x.SubTotal).HasColumnType("decimal(10,2)");
            e.Property(x => x.DeliveryFee).HasColumnType("decimal(10,2)");
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.HasMany(x => x.Items)
             .WithOne(x => x.Order)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UnitPrice).HasColumnType("decimal(10,2)");
            e.Property(x => x.TotalPrice).HasColumnType("decimal(10,2)");
            e.Property(x => x.Quantity).HasColumnType("decimal(10,2)");
        });
    }
}
