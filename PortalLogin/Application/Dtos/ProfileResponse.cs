namespace Application.Dtos;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string ProfileName { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
}