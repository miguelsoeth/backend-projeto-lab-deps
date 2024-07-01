using Application.Contract;
using Application.Dtos;
using Application.Dtos.Account;
using Application.Dtos.Consulta;
using Application.Interfaces;
using Domain.Entities;
using Domain.Util;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ConsultaController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly  ICreditRepository _creditRepository;
    private readonly IVendaRepository _vendaRepository;
    private readonly IPublisherService _publisherService;

    public ConsultaController(IUserRepository userRepository, ICreditRepository creditRepository, IVendaRepository vendaRepository, IPublisherService publisherService)
    {
        _userRepository = userRepository;
        _creditRepository = creditRepository;
        _vendaRepository = vendaRepository;
        _publisherService = publisherService;
    }

    [HttpPost("online")]
    public async Task<ActionResult<AuthResponseDto>> ConsultarOnline(ConsultaOnlineDto consultaDto)
    {
        var docValido = ValidateDocument.CpfCnpj(consultaDto.documento);
        if (!docValido) { return BadRequest(new { IsSuccess = false, Message="Documento inválido" }); }

        var venda = await _vendaRepository.GetSaleById(consultaDto.venda);
        if (venda == null) { return BadRequest(new { IsSuccess = false, Message = "Venda não encontrada" }); }
        if (!venda.ProductActive) { return BadRequest(new { IsSuccess = false, Message = "Venda não ativa" }); }

        var userCredtis = await _creditRepository.GetCreditAsync(consultaDto.usuario);
        if (userCredtis.Amount < venda.Valor) { return BadRequest(new { IsSuccess = false, Message = "Créditos insuficientes!" }); }
        
        //return Ok(new { IsSuccess = true, Message="OK", obj=consultaDto });

        var response = await _publisherService.ConsultarOnline("teste", consultaDto);
        return Ok(new { IsSuccess = true, Message="OK", Response=response });
    }
    
    [HttpPost("lote")]
    public async Task<ActionResult<AuthResponseDto>> ConsultarLote(ConsultaLoteDto consultaDto)
    {
        foreach (var documento in consultaDto.documentos)
        {
            if (!ValidateDocument.CpfCnpj(documento))
            {
                return BadRequest(new { IsSuccess = false, Message=$"Documento inválido, {documento}" });
            }
        }
        

        var venda = await _vendaRepository.GetSaleById(consultaDto.venda);
        if (venda == null) { return BadRequest(new { IsSuccess = false, Message = "Venda não encontrada" }); }
        if (!venda.ProductActive) { return BadRequest(new { IsSuccess = false, Message = "Venda não ativa" }); }

        var userCredtis = await _creditRepository.GetCreditAsync(consultaDto.usuario);
        if (userCredtis.Amount < venda.Valor) { return BadRequest(new { IsSuccess = false, Message = "Créditos insuficientes!" }); }
        
        //return Ok(new { IsSuccess = true, Message="OK", obj=consultaDto });
        
        await _publisherService.ConsultarLote("teste", consultaDto);
        return Ok(new { IsSuccess = true, Message="OK" });
    }
}