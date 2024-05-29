using System.Reflection.Metadata.Ecma335;

namespace Domain.Entities;

public class Credit
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    
    //relação de navegação
    public ApplicationUser User { get; set; }
}