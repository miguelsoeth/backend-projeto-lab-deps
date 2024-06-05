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

  [HttpGet("GetSales")]
  public async Task<ActionResult> GetAllSalesByIdUser(string id)
  {
    if (!Guid.TryParse(id, out Guid userGuid))
    {
      return BadRequest("O formato do id é inválido");
    }
    var result = await _vendaRepository.GetSaleByUserId(userGuid);
    if (result == null) return BadRequest("Erro ao buscas as vendas");
    return Ok(result);
  }
  
  
}