using Application.Dtos.Consulta;

namespace Application.Interfaces;

public interface IPublisherService
{
    Task ConsultarLote(string fila, ConsultaLoteDto request);
    
    Task<ConsultaResponseDto> ConsultarOnline(string fila, ConsultaOnlineDto request);
}