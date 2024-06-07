namespace Application.Dtos;

public class SaleDto
{
    public Guid SaleId { get; set; }
    public string? SaleName { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductDescription { get; set; }
    public bool ProductActive { get; set; }
    public decimal Valor { get; set; } = 0.0m;
}