using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class EditProfileDto
{
    [Required]
    public string? ProfileName { get; set; }
}