using System.Security.AccessControl;

namespace Domain.Entities;

public class Venda
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Valor { get; set; } = 0.0m;
    public bool isActive { get; set; }
    
    public ApplicationUser User { get; set; }
    public Products Product { get; set; }  
    
}