using Application.Dtos.Consulta;
using Application.Interfaces;
using MassTransit;

namespace Infrastructure.Messaging;

public class PublisherService : IPublisherService
{
    private readonly IBus _bus;

    public PublisherService(IBus bus)
    {
        _bus = bus;
    }

    public async Task ConsultarLote(string fila, ConsultaLoteDto request)
    {
        var uri = new Uri($"rabbitmq://localhost/{fila}");
        var endPoint = await _bus.GetSendEndpoint(uri);
        await endPoint.Send(request);
    }

    public async Task<ConsultaResponseDto> ConsultarOnline(string fila, ConsultaOnlineDto request)
    {
        var uri = new Uri($"rabbitmq://localhost/{fila}");
        var client = _bus.CreateRequestClient<ConsultaOnlineDto>(uri);
        Response<ConsultaResponseDto> response;
        try
        {
            response = await client.GetResponse<ConsultaResponseDto>(request, default,
                RequestTimeout.After(0, 0, 1, 0, 0));
        }
        catch (Exception a)
        {
            return null;
        }

        return response.Message;
    }
}