using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Profile;

public class ProfileDto
{
    [Required]
    public string ProfileName { get; set; }
    public string? userId { get; set; }
}