using System.Net;
using Application.Dtos;
using Application.Dtos.Account;
using Domain.Entities;

namespace Application.Contract;

public interface IVendaRepository
{
    Task<AuthResponseDto> CreateSaleAsync(SaleDto sale);
    Task<AuthResponseDto> EditSaleAsync(string id, SaleDto sale);
    Task<List<SaleDto>> GetSaleByUserId(Guid id);
}