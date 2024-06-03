using System.Security.AccessControl;

namespace Domain.Entities;

public class Venda
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Valor { get; set; }
    
    public ApplicationUser User { get; set; }  // Adicionado
    public Products Product { get; set; }  // Adicionado
    
}