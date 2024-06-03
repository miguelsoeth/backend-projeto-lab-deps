using System.Security.AccessControl;

namespace Domain.Entities;

public class Produts
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Descricao { get; set; }
}