namespace Application.Dtos;

public class UserDetailDto
{
    public Guid Id { get;  set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Document { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; }
    public List<string>? Roles { get; set; }
}