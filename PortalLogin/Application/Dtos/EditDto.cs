using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class EditDto
{
    public string? Name { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; }
    public List<string>? Roles { get; set; }
 
}