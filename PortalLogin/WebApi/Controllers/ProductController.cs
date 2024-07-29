using Application.Contract;
using Application.Dtos.Account;
using Application.Dtos.Products;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpPost("CreateProduct")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateProduct([FromBody] ProductDto productDto)
    {
        var response = await _productRepository.CreateProductsAsync(productDto);
        return Ok(response);
    }

    [HttpPut("EditProducts")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> EditProduct(Guid id, [FromBody] ProductDto productDto)
    {
        var response = await _productRepository.EditProductAsync(id, productDto);
        return Ok(response);
    }

    [HttpDelete("DeleteProducts/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var response = await _productRepository.DeleteProductAsync(id);
        return Ok(response);
    }

    [HttpGet("GetProducts/{id}")]
    [Authorize]
    public async Task<ActionResult> GetProductById(Guid id)
    {
        var response = await _productRepository.GetProductByIdAsync(id);
        return Ok(response);
    }

    [HttpGet("GetProducts")]
    [Authorize]
    public async Task<ActionResult> GetAllProducts()
    {
        var response = await _productRepository.GetAllProductsAsync();
        return Ok(response);
    }
}