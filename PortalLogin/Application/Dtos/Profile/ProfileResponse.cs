namespace Application.Dtos.Profile;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ProfileName { get; set; }
    public string Message { get; set; }
}