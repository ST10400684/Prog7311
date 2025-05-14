using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Prog.Models;

public partial class AgriEnergyConnectContext : DbContext
{
    public AgriEnergyConnectContext()
    {
    }

    public AgriEnergyConnectContext(DbContextOptions<AgriEnergyConnectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Farmer> Farmers { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductFullView> ProductFullViews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=AgriEnergyConnect.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Farmer>(entity =>
        {
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");

            entity.HasOne(d => d.User).WithMany(p => p.Farmers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
            entity.Property(e => e.ProductionDate).HasColumnType("DATETIME");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Farmer).WithMany(p => p.Products).HasForeignKey(d => d.FarmerId);
        });

        //Extra stuf----------- index for frequently queried columns
        modelBuilder.Entity<Product>()
       .HasIndex(p => p.ProductionDate);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.FarmerId);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.CategoryId);

        modelBuilder.Entity<Farmer>()
            .HasIndex(f => f.UserId);

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.HasIndex(e => e.CategoryName, "IX_ProductCategories_CategoryName").IsUnique();
        });

        modelBuilder.Entity<ProductFullView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ProductFullView");

            entity.Property(e => e.CreatedDate).HasColumnType("DATETIME");
            entity.Property(e => e.ProductionDate).HasColumnType("DATETIME");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username, "IX_Users_Username").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
            entity.Property(e => e.LastLogin).HasColumnType("DATETIME");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
