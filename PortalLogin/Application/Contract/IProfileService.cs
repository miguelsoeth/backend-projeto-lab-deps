using Application.Dtos;

namespace Application.Contract;

public interface IProfileService
{
    Task<ProfileResponse> CreateProfileAsync(string userId, ProfileDto profileDto);
    Task<ListUserProfileDto> GetUserProfileByIdAsync(Guid userId);
    Task<EditProfileResponse> EditProfileByIdAsync(Guid id, EditProfileDto editProfileDto);
    Task<EditProfileResponse> ListProfileByIdAsync(Guid id);
}