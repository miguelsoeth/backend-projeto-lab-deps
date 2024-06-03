using Application.Dtos;
using Application.Dtos.Profile;

namespace Application.Contract;

public interface IProfileRepository
{
    Task<ProfileResponse> CreateProfileAsync(Guid userId, ProfileDto profileDto);
    Task<List<ProfileResponse>> GetUserProfilesByIdAsync(Guid userId);
    Task<ProfileResponse> EditProfileByIdAsync(Guid id, ProfileDto editProfileDto);
    Task<ProfileResponse> DeleteProfileByIdAsync(Guid id);
    
    //Task<EditProfileResponse> ListProfileByIdAsync(Guid id);
}