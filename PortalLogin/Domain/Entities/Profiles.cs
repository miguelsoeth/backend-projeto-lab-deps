namespace Domain.Entities;

public class Profiles
{
    public Guid Id { get; set; }
    public string? ProfileName { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser applicationUser { get; set; }
}
