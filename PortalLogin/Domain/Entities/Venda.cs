using System.Security.AccessControl;

namespace Domain.Entities;

public class Venda
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProdutId { get; set; }
    public decimal Valor { get; set; }
}