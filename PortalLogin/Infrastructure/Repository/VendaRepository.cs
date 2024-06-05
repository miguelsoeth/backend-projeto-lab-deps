using Application.Contract;
using Application.Dtos;
using Application.Dtos.Account;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class VendaRepository : IVendaRepository
{
    private readonly AppDbContext _appDbContext;

    public VendaRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<AuthResponseDto> CreateSaleAsync(SaleDto sale)
    {
        var produto = await _appDbContext.Produtos.FindAsync(sale.ProductId);
        var user = await _appDbContext.Users.FindAsync(sale.UserId);
        if (produto == null) 
            return new AuthResponseDto { IsSuccess = false, Message = "Id do produto inválido!"};
        if (user == null) 
            return new AuthResponseDto { IsSuccess = false, Message = "Id do usuário inválido!" };
        
        string? reason = null;
        if (sale.UserId == Guid.Empty) reason = "Obrigatório informar o usuário!";
        if (sale.ProductId == Guid.Empty) reason = "Obrigatório informar o produto!";
        if (sale.Valor == null || sale.Valor <= 0) reason = "Obrigatório infomar o valor do produto!";
        if (reason != null) return new AuthResponseDto
            {
                IsSuccess = false,
                Message = reason
            };

        _appDbContext.Vendas.Add(new Venda()
        {
            UserId = sale.UserId,
            ProductId = sale.ProductId,
            Valor = sale.Valor
        });
        await _appDbContext.SaveChangesAsync();

        return new AuthResponseDto{ IsSuccess = true, Message = "Venda criado com sucesso!" };

    }

    public async Task<AuthResponseDto> EditSaleAsync(string id, SaleDto sale)
    {
        var venda = await _appDbContext.Vendas.FindAsync(id);
        if (venda == null) return new AuthResponseDto { IsSuccess = false, Message = "Venda não encotrado" };
        
        string? reason = null;
        if (sale.UserId == Guid.Empty) reason = "Obrigatório informa o usuário!";
        if (sale.ProductId == Guid.Empty) reason = "Obrigatório informar o produto!";
        if (sale.Valor == null) reason = "Obrigatório infomar o valor do produto!";
        if (reason != null) return new AuthResponseDto
        {
            IsSuccess = false,
            Message = reason
        };

        if (!string.IsNullOrEmpty(sale.UserId.ToString())) venda.UserId = sale.UserId;
        if (!string.IsNullOrEmpty(sale.ProductId.ToString())) venda.ProductId = sale.ProductId;
        if (!string.IsNullOrEmpty(sale.Valor.ToString())) venda.Valor = sale.Valor;

        _appDbContext.Vendas.Update(venda);
        await _appDbContext.SaveChangesAsync();
        return new AuthResponseDto { IsSuccess = true, Message = "Venda editada com sucesso!"};
    }
    
    public async Task<List<SaleDto>> GetSaleByUserId(Guid id)
    {
        var user = await _appDbContext.Users.FindAsync(id);
        if (user == null) return null;
        
        var vendas = await _appDbContext.Vendas
            .Where(v => v.UserId == id)
            .Select(v => new SaleDto
            {
                UserId = v.UserId,
                ProductId = v.ProductId,
                Valor = v.Valor
            })
            .ToListAsync();

        if (vendas == null || !vendas.Any())
            return null;
        
        return vendas;
    }
    
}