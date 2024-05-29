namespace Domain.Entities;

public class ApplicationUser
{
    public Guid Id { get;  set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Document { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; }
    public List<string>? Roles { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    
    //Relações da tabela
    public ICollection<Profiles>? Profiles { get; set; }
    public ICollection<Credit>? Credits { get; set; }
}