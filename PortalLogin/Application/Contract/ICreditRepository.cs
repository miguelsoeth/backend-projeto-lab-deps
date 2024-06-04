using Application.Dtos.Account;
using Application.Dtos.Credits;

namespace Application.Contract;

public interface ICreditRepository
{
    Task<CreditDto> GetCreditAsync(Guid userId);
    Task<AuthResponseDto> IncreaseCreditAsync(Guid userId, decimal amount);
    Task<AuthResponseDto> DecreaseCreditAsync(Guid userId, decimal amount);
}