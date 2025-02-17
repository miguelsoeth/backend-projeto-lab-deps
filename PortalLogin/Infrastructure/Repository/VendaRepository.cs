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

    private async Task<string> GenerateNewSaleNameAsync()
    {
        var lastSale = await _appDbContext.Vendas
            .OrderByDescending(v => v.Name)
            .FirstOrDefaultAsync();

        int newSequencial = 1;

        if (lastSale != null)
        {
            string lastName = lastSale.Name;
            if (lastName.StartsWith("Deps ") && int.TryParse(lastName.Substring(5), out int lastSequencial))
            {
                newSequencial = lastSequencial + 1;
            }
        }

        return $"Deps {newSequencial:000}";
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

        string newName = await GenerateNewSaleNameAsync();

        _appDbContext.Vendas.Add(new Venda()
        {
            Name = newName,
            UserId = sale.UserId,
            ProductId = sale.ProductId,
            Valor = sale.Valor,
            isActive = true
        });
        await _appDbContext.SaveChangesAsync();

        return new AuthResponseDto{ IsSuccess = true, Message = "Venda criado com sucesso!" };

    }

    public async Task<AuthResponseDto> DisableSaleAsync(string id, bool isActive)
    {
        var venda = await _appDbContext.Vendas.FindAsync(Guid.Parse(id));
        if (venda == null) return new AuthResponseDto { IsSuccess = false, Message = "Venda não encotrada" };
        
        if (isActive == null) return new AuthResponseDto
        {
            IsSuccess = false,
            Message = "Obrigatório infomar o estado do produto!"
        };
        
        venda.isActive = isActive;

        _appDbContext.Vendas.Update(venda);
        await _appDbContext.SaveChangesAsync();
        return new AuthResponseDto { IsSuccess = true, Message = "Venda editada com sucesso!"};
    }

    public async Task<AuthResponseDto> DeleteSaleAsync(string id)
    {
        var venda = await _appDbContext.Vendas.FindAsync(Guid.Parse(id));
        if (venda == null) return new AuthResponseDto { IsSuccess = false, Message = "Venda não encotrada" };

        _appDbContext.Vendas.Remove(venda);
        await _appDbContext.SaveChangesAsync();
        return new AuthResponseDto { IsSuccess = true, Message = "Venda removida com sucesso!"};
    }

    public async Task<List<SaleDto>> GetSaleByUserId(Guid id, bool onlyActives)
    {
        var user = await _appDbContext.Users.FindAsync(id);
        if (user == null) return null;

        var query = _appDbContext.Vendas.Where(v => v.UserId == id);

        if (onlyActives)
        {
            var vendas = await query
                .Where(v => v.isActive)
                .Select(v => new SaleDto
                {
                    SaleId = v.Id,
                    SaleName = v.Name,
                    ProductId = v.ProductId,
                    ProductName = v.Product.Name,
                    ProductDescription = v.Product.Descricao,
                    ProductActive = v.isActive,
                    Valor = v.Valor
                })
                .ToListAsync();

            return vendas;
        }
        else
        {
            var vendas = await query
                .Select(v => new SaleDto
                {
                    SaleId = v.Id,
                    SaleName = v.Name,
                    ProductId = v.ProductId,
                    ProductName = v.Product.Name,
                    ProductDescription = v.Product.Descricao,
                    ProductActive = v.isActive,
                    Valor = v.Valor
                })
                .ToListAsync();

            return vendas;
        }
    }
    
    public async Task<SaleDto> GetSaleById(Guid id)
    {
        var query = _appDbContext.Vendas.Where(v => v.Id == id);

        var vendas = await query
            .Select(v => new SaleDto
            {
                SaleId = v.Id,
                SaleName = v.Name,
                ProductId = v.ProductId,
                ProductName = v.Product.Name,
                ProductDescription = v.Product.Descricao,
                ProductActive = v.isActive,
                Valor = v.Valor
            })
            .FirstOrDefaultAsync();

        return vendas;
    }
    
}