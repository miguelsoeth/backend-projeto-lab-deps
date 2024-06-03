using Application.Contract;
using Application.Dtos.Account;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class CreditRepository : ICreditRepository
{

    private readonly AppDbContext _appDbContext;

    public CreditRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<AuthResponseDto> IncreaseCreditAsync(Guid userId, decimal amount)
    {
        var user = await _appDbContext.Users.Include(u => u.Credits).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Usuário não encontrado!"
            };
        }

        var currentCredit = user.Credits.FirstOrDefault();
        if (currentCredit == null)
        {
            currentCredit = new Credit
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 0,
                TransactionDate = DateTime.UtcNow
            };
            _appDbContext.Credits.Add(currentCredit);
        }

        currentCredit.Amount += amount;
        currentCredit.TransactionDate = DateTime.UtcNow;

        await _appDbContext.SaveChangesAsync();

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Créditos adicionados com sucesso!"
        };
    }

    public async Task<AuthResponseDto> DecreaseCreditAsync(Guid userId, decimal amount)
    {
        var user = await _appDbContext.Users.Include(u => u.Credits).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Usuário não encontrado!"
            };
        }

        var currentCredit = user.Credits.FirstOrDefault();
        if (currentCredit == null || currentCredit.Amount < amount)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Créditos insuficientes!"
            };
        }

        currentCredit.Amount -= amount;
        currentCredit.TransactionDate = DateTime.UtcNow;

        await _appDbContext.SaveChangesAsync();

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Créditos retirados com sucesso!"
        };
    }
}