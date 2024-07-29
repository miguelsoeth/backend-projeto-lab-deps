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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateSale(SaleDto sale)
    {
        var result = await _vendaRepository.CreateSaleAsync(sale);
        if (result.IsSuccess == false)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("disable/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DisableSale(string id, bool isActive)
    {
        var result = await _vendaRepository.DisableSaleAsync(id, isActive);
        if (result.IsSuccess == false)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteSale(string id)
    {
        var result = await _vendaRepository.DeleteSaleAsync(id);
        if (result.IsSuccess == false)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }


    [HttpGet("GetSales")]
    [Authorize]
    public async Task<ActionResult> GetAllSalesByIdUser(string id)
    {
        if (!Guid.TryParse(id, out Guid userGuid))
        {
            return BadRequest("O formato do id é inválido");
        }

        var result = await _vendaRepository.GetSaleByUserId(userGuid, !HttpContext.User.IsInRole("Admin"));
        if (result == null) return NotFound("Usuário não encontrado!");
        return Ok(result);
    }
}