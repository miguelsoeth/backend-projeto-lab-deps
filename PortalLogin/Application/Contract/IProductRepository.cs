using Application.Dtos.Account;
using Application.Dtos.Products;
using Domain.Entities;

namespace Application.Contract;

public interface IProductRepository
{
    Task<List<Products>> GetAllProductsAsync();
    Task<AuthResponseDto> CreateProductsAsync(ProductDto productDto);
    Task<AuthResponseDto> EditProductAsync(Guid id, ProductDto productDto);
    Task<AuthResponseDto> DeleteProductAsync(Guid id);
    Task<Products?> GetProductByIdAsync(Guid id);
}