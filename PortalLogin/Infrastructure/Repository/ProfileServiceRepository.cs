using System.Security.Claims;
using Application.Contract;
using Application.Dtos;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Repository;

public class ProfileServiceRepository : IProfileService
{

    private readonly AppDbContext _appDbContext;
    private readonly IHttpContextAccessor _contextAccessor;

    public ProfileServiceRepository(IHttpContextAccessor contextAccessor, AppDbContext appDbContext)
    {
        _contextAccessor = contextAccessor;
        _appDbContext = appDbContext;
    }

    public async Task<ProfileResponse> CreateProfileAsync(string userId, ProfileDto profileDto)
    {
        var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userNameClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

        if (userIdClaim == null && userNameClaim == null)
        {
            throw new UnauthorizedAccessException("User is not Authenticated");
        }

        var user = await _appDbContext.Users.FindAsync(Guid.Parse(userIdClaim));

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var profile = new Profiles
        {
            ProfileName = profileDto.ProfileName,
            UserId = user.Id,
            applicationUser = user
        };

        _appDbContext.Profiles.Add(profile);
        await _appDbContext.SaveChangesAsync();
        
        return new ProfileResponse
        {
            Id = profile.Id,
            ProfileName = profileDto.ProfileName,
            UserId = profile.UserId,
            UserName = user.Name
        };
    }
}