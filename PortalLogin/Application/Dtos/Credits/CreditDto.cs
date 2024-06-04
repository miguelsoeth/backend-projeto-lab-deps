namespace Application.Dtos.Credits;

public class CreditDto
{
    public Guid? UserId { get; set; }
    public decimal? Amount { get; set; }
    public string? Message { get; set; }
}