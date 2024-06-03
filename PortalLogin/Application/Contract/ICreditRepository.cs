using Application.Dtos.Account;

namespace Application.Contract;

public interface ICreditRepository
{
    Task<AuthResponseDto> IncreaseCreditAsync(Guid userId, decimal amount);
    Task<AuthResponseDto> DecreaseCreditAsync(Guid userId, decimal amount);
}