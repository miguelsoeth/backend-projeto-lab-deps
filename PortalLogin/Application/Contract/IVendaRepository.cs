using System.Net;
using Application.Dtos;
using Application.Dtos.Account;
using Domain.Entities;

namespace Application.Contract;

public interface IVendaRepository
{
    Task<AuthResponseDto> CreateSaleAsync(SaleDto sale);
    Task<AuthResponseDto> DisableSaleAsync(string id, bool isActive);
    Task<AuthResponseDto> DeleteSaleAsync(string id);
    Task<List<SaleDto>> GetSaleByUserId(Guid id, bool onlyActives);
    Task<SaleDto> GetSaleById(Guid id);
}