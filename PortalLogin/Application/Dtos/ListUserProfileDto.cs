namespace Application.Dtos;

public class ListUserProfileDto
{
    public Guid idProfile { get; set; }
    public string? ProfileName { get; set; }
}

public class ListProfileDto
{
    public List<ListUserProfileDto> Profiles { get; set; }
}