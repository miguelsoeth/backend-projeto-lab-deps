namespace Application.Dtos.Consulta;

public class ConsultaLoteDto
{
    public string usuario { get; set; }
    public Guid venda { get; set; }
    public List<string> documentos { get; set; }
    public string perfil { get; set; }
}