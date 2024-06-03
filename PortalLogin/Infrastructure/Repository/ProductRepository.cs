using Application.Contract;
using Application.Dtos.Account;
using Application.Dtos.Products;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Repository;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _appDbContext;

    public ProductRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }


    public async Task<List<Products>> GetAllProductsAsync()
    {
        return await _appDbContext.Produtos.ToListAsync();
    }

    public async Task<AuthResponseDto> CreateProductsAsync(ProductDto productDto)
    {
        var existProduct = await _appDbContext.Produtos.FirstOrDefaultAsync(p => p.Name == productDto.Nome);
        if (existProduct != null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Já existe um produto com esse nome!"
            };
        }
        var product = new Products
        {
            Id = Guid.NewGuid(),
            Name = productDto.Nome,
            Descricao = productDto.Descicao
        };

        _appDbContext.Produtos.Add(product);
        await _appDbContext.SaveChangesAsync();

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Produto criado com sucesso!"
        };

    }

    public async Task<AuthResponseDto> EditProductAsync(Guid id, ProductDto productDto)
    {
        // Verificar se o DTO de produto não é nulo
        if (productDto == null)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Os dados do produto não podem ser nulos" };
        }

        var product = await _appDbContext.Produtos.FindAsync(id);
        if (product == null)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Produto não encontrado" };
        }
    
        // Verificar se ambos os campos estão vazios
        if (string.IsNullOrWhiteSpace(productDto.Nome) && string.IsNullOrWhiteSpace(productDto.Descicao))
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Os dados do produto não podem ser atualizados para vazio" };
        }

        // Atualizar os campos apenas se não estiverem vazios
        if (!string.IsNullOrWhiteSpace(productDto.Nome))
        {
            product.Name = productDto.Nome;
        }
        if (!string.IsNullOrWhiteSpace(productDto.Descicao))
        {
            product.Descricao = productDto.Descicao;
        }

        _appDbContext.Produtos.Update(product);
        await _appDbContext.SaveChangesAsync();

        return new AuthResponseDto { IsSuccess = true, Message = "Produto atualizado com sucesso" };
    }


    public async Task<AuthResponseDto> DeleteProductAsync(Guid id)
    {
        var product = await _appDbContext.Produtos.FindAsync(id);
        if (product == null)
            return new AuthResponseDto { IsSuccess = false, Message = "Produto não encontrado!" };

        _appDbContext.Produtos.Remove(product);
        await _appDbContext.SaveChangesAsync();

        return new AuthResponseDto { IsSuccess = true, Message = "Produto deletado com sucesso" };
    }

    public async Task<Products?> GetProductByIdAsync(Guid id)
    {
        return await _appDbContext.Produtos.FindAsync(id);
    }
}