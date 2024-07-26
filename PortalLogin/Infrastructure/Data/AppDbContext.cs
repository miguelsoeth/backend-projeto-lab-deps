using Domain.Entities;
using MassTransit;
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
    public DbSet<Products> Produtos { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Profiles>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<Venda>()
            .Property(v => v.isActive)
            .HasDefaultValue(true);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Profiles)
            .WithOne(p => p.applicationUser)
            .HasForeignKey(p => p.UserId);

        // Configuração da relação um para muitos entre ApplicationUser e Credit
        modelBuilder.Entity<Credit>()
            .HasOne(c => c.User)
            .WithMany(u => u.Credits)
            .HasForeignKey(c => c.UserId);
        
        modelBuilder.Entity<Venda>()
            .HasOne(v => v.User)
            .WithMany(u => u.Vendas)
            .HasForeignKey(v => v.UserId);

        modelBuilder.Entity<Venda>()
            .HasOne(v => v.Product)
            .WithMany(p => p.Vendas)
            .HasForeignKey(v => v.ProductId);
        
        modelBuilder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Name = "Administrador",
                Email = "admin@mail",
                Document = "11122233344",
                Password = "$2a$11$EqhXhYgMJyjOCOV/syecku/WQ1z5/T/Nvso6SNogKdxdP.qd5Tqae", // Note: in a real application, passwords should be hashed
                IsActive = true,
                Roles = new List<string> { "Admin" },
                RefreshToken = null,
                RefreshTokenExpiryTime = DateTime.MinValue
            }
        );
        
        modelBuilder.Entity<Products>().HasData(
            new Products
            {
                Id = Guid.NewGuid(),
                Name = "Dados Publicos",
                Descricao = "Consulta em dados publicos da Deps",
                IsActive = true
            }
        );

        base.OnModelCreating(modelBuilder);
    }
}