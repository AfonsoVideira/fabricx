using InteractionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InteractionApi.Data;

public class InteractionDbContext : DbContext
{
    public InteractionDbContext(DbContextOptions<InteractionDbContext> options) : base(options)
    {
    }

    public DbSet<Skill> Skills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Skill entity
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Seed default skills
        modelBuilder.Entity<Skill>().HasData(
            new Skill { Id = 1, Name = "Sales", Description = "Customer sales and upselling", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Skill { Id = 2, Name = "Support", Description = "Technical customer support", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Skill { Id = 3, Name = "Billing", Description = "Billing and payment inquiries", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Skill { Id = 4, Name = "Technical", Description = "Advanced technical troubleshooting", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Skill { Id = 5, Name = "Retention", Description = "Customer retention and winback", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
    }
} 