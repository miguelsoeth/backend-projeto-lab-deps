using Application.Dtos.Consulta;

namespace Application.Interfaces;

public interface IPublisherService
{
    Task ConsultarLote(string fila, ConsultaOnlineDto consulta);
    
    Task<ConsultaResponseDto> ConsultarOnline(string fila, ConsultaOnlineDto request);
}