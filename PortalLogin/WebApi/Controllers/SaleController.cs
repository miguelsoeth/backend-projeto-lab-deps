using Application.Contract;
using Application.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class SaleController : ControllerBase
{
  private readonly IVendaRepository _vendaRepository;

  public SaleController(IVendaRepository vendaRepository)
  {
    _vendaRepository = vendaRepository;
  }

  [HttpPost("CreateSale")]
  public async Task<ActionResult> CreateSale(SaleDto sale)
  {
    var result = await _vendaRepository.CreateSaleAsync(sale);
    if (result.IsSuccess == false)
    {
      return BadRequest(result);
    }
    return Ok(result);
  }

  [HttpPut("EditSale/{id}")]
  public async Task<ActionResult> EditSale(string id, SaleDto sale)
  {
    var result = await _vendaRepository.EditSaleAsync(id, sale);
    if (result.IsSuccess == false)
    {
      return BadRequest(result);
    }
    return Ok(result);
  }
  
  
}