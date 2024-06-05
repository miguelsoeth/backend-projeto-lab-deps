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
    public async Task<ActionResult> CreateProduct([FromBody] ProductDto productDto)
    {
        var response = await _productRepository.CreateProductsAsync(productDto);
        if (response.IsSuccess == false) return BadRequest(response.Message);
        return Ok(response.Message);
    }

    [HttpPut("EditProducts")]
    public async Task<ActionResult> EditProduct(Guid id, [FromBody] ProductDto productDto)
    {
        var response = await _productRepository.EditProductAsync(id, productDto);
        if (response.IsSuccess == false) return BadRequest(response.Message);
        return Ok(response.Message);
    }

    [HttpDelete("DeleteProducts/{id}")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var response = await _productRepository.DeleteProductAsync(id);
        if (response.IsSuccess == false) return BadRequest(response.Message);
            
        return Ok(response);
    }

    [HttpGet("GetProducts/{id}")]
    public async Task<ActionResult> GetProductById(Guid id)
    {
        var response = await _productRepository.GetProductByIdAsync(id);
        return Ok(response);
    }

    [HttpGet("GetProducts")]
    public async Task<ActionResult> GetAllProducts()
    {
        var response = await _productRepository.GetAllProductsAsync();
        return Ok(response);
    }
}