using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class ProfileDto
{
    [Required]
    public string ProfileName { get; set; }
}