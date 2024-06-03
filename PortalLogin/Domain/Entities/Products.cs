using System.Security.AccessControl;

namespace Domain.Entities;

public class Products
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Descricao { get; set; }
    public ICollection<Venda>? Vendas { get; set; }
}