using AgentStateApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentStateApi.Data;

public class AgentStateDbContext : DbContext
{
    public AgentStateDbContext(DbContextOptions<AgentStateDbContext> options) : base(options)
    {
    }
    
    public DbSet<Agent> Agents { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).IsRequired(); // Soft foreign key to AuthService.Users.Id
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Skills).HasMaxLength(500); // JSON string for skills
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.UserId).IsUnique(); // One agent per user
        });
        
        // Seed data - using known user IDs from AuthService
        modelBuilder.Entity<Agent>().HasData(
            new Agent 
            { 
                Id = 1, 
                Name = "Admin User", 
                UserId = 1, // References admin user from AuthService
                State = "Available", 
                LastStateChange = DateTime.UtcNow 
            },
            new Agent 
            { 
                Id = 2, 
                Name = "Agent User", 
                UserId = 2, // References agent user from AuthService
                State = "Busy", 
                LastStateChange = DateTime.UtcNow 
            }
        );
    }
} 