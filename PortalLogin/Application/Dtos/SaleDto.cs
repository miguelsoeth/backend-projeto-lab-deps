namespace Application.Dtos;

public class SaleDto
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Valor { get; set; } = 0.0m;
}