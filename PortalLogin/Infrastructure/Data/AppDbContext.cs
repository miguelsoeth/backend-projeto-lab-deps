using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Profiles> Profiles { get; set; }
    public DbSet<Credit> Credits { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Profiles>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Profiles)
            .WithOne(p => p.applicationUser)
            .HasForeignKey(p => p.UserId);

        // Configuração da relação um para muitos entre ApplicationUser e Credit
        modelBuilder.Entity<Credit>()
            .HasOne(c => c.User)
            .WithMany(u => u.Credits)
            .HasForeignKey(c => c.UserId);

        base.OnModelCreating(modelBuilder);
    }
}