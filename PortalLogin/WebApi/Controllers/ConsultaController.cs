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
    private readonly ICreditRepository _creditRepository;
    private readonly IVendaRepository _vendaRepository;
    private readonly IPublisherService _publisherService;

    public ConsultaController(IUserRepository userRepository, ICreditRepository creditRepository,
        IVendaRepository vendaRepository, IPublisherService publisherService)
    {
        _userRepository = userRepository;
        _creditRepository = creditRepository;
        _vendaRepository = vendaRepository;
        _publisherService = publisherService;
    }

    [HttpPost("online")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ConsultaResponseDto>> ConsultarOnline(ConsultaOnlineDto consultaDto)
    {
        var docValido = ValidateDocument.CpfCnpj(consultaDto.documento);
        if (!docValido)
        {
            return BadRequest(new ConsultaResponseDto { Success = false, Message = "Documento inválido" });
        }

        var venda = await _vendaRepository.GetSaleById(consultaDto.venda);
        if (venda == null)
        {
            return BadRequest(new ConsultaResponseDto { Success = false, Message = "Venda não encontrada" });
        }

        if (!venda.ProductActive)
        {
            return BadRequest(new ConsultaResponseDto { Success = false, Message = "Venda não ativa" });
        }

        var userCredtis = await _creditRepository.GetCreditAsync(Guid.Parse(consultaDto.usuario));
        if (userCredtis.Amount < venda.Valor)
        {
            return BadRequest(new ConsultaResponseDto { Success = false, Message = "Créditos insuficientes!" });
        }

        var userDetail = await _userRepository.GetUserByIdAsync(Guid.Parse(consultaDto.usuario));

        //return Ok(new { IsSuccess = true, Message="OK", obj=consultaDto });
        consultaDto.dataCadastro = DateTime.Now;
        consultaDto.usuarioId = consultaDto.usuario;
        consultaDto.usuario = userDetail.Name;

        var response = await _publisherService.ConsultarOnline("fila-online", consultaDto);
        if (response.Success)
        {
            await _creditRepository.DecreaseCreditAsync(Guid.Parse(consultaDto.usuarioId), venda.Valor);
        }

        return Ok(response);
    }

    [HttpPost("lote")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ConsultaResponseDto>> ConsultarLote(ConsultaLoteDto consultaDto)
    {
        foreach (var documento in consultaDto.documentos)
        {
            if (!ValidateDocument.CpfCnpj(documento))
            {
                return BadRequest(new { IsSuccess = false, Message = $"Documento inválido, {documento}" });
            }
        }


        var venda = await _vendaRepository.GetSaleById(consultaDto.venda);
        if (venda == null)
        {
            return BadRequest(new { IsSuccess = false, Message = "Venda não encontrada" });
        }

        if (!venda.ProductActive)
        {
            return BadRequest(new { IsSuccess = false, Message = "Venda não ativa" });
        }

        var userCredtis = await _creditRepository.GetCreditAsync(Guid.Parse(consultaDto.usuario));
        var valorTotal = venda.Valor * consultaDto.documentos.Count;
        if (userCredtis.Amount < valorTotal)
        {
            return BadRequest(new { IsSuccess = false, Message = "Créditos insuficientes!" });
        }

        var userDetail = await _userRepository.GetUserByIdAsync(Guid.Parse(consultaDto.usuario));

        //return Ok(new { IsSuccess = true, Message="OK", obj=consultaDto });
        int quant = consultaDto.documentos.Count;
        string batch_id = Guid.NewGuid().ToString();

        foreach (var documento in consultaDto.documentos)
        {
            var consulta = new ConsultaOnlineDto
            {
                usuario = userDetail.Name,
                usuarioId = consultaDto.usuario,
                venda = consultaDto.venda,
                lote = batch_id,
                quantidade = quant,
                documento = documento,
                perfil = consultaDto.perfil,
                dataCadastro = DateTime.Now
            };
            Console.WriteLine(consulta.usuarioId);
            _publisherService.ConsultarLote("fila-lote", consulta);
        }

        await _creditRepository.DecreaseCreditAsync(Guid.Parse(consultaDto.usuario), valorTotal);
        return Ok(new ConsultaResponseDto { Success = true, Message = "Lote enviado com sucesso!" });
    }
}