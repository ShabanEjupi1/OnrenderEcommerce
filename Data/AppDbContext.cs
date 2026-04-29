using ProjectTemplate.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectTemplate.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<AnswerRecord> AnswerRecords => Set<AnswerRecord>();
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();

    // YourBrand Integration
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<PosSystem> PosSystems => Set<PosSystem>();
    public DbSet<ProductItem> ProductItems => Set<ProductItem>();
    public DbSet<POSOrder> POSOrders => Set<POSOrder>();
    public DbSet<PosStaff> PosStaff => Set<PosStaff>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    // New Tables
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
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

        // YourBrand Business Data Modeling
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
    }
}

