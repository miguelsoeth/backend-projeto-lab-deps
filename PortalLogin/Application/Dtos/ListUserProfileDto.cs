namespace Application.Dtos;

public class ListUserProfileDto
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string[]? Profiles { get; set; }
}