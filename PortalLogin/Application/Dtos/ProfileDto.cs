using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;

namespace Application.Dtos;

public class ProfileDto
{
    [Required]
    public string ProfileName { get; set; }
}