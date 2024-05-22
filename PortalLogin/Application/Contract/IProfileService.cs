using Application.Dtos;

namespace Application.Contract;

public interface IProfileService
{
    Task<ProfileResponse> CreateProfileAsync(string userId, ProfileDto profileDto);
    Task<ListUserProfileDto> GetUserProfileByIdAsync(Guid userId);
}