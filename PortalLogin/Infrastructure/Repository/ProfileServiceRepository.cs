using System.Security.Claims;
using Application.Contract;
using Application.Dtos;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }

        var user = await _appDbContext.Users.FindAsync(Guid.Parse(userIdClaim));

        if (user == null)
        {
            throw new Exception("Usuário não encontrado");
        }

        var nameExist = await _appDbContext.Profiles.AnyAsync(u => u.ProfileName == profileDto.ProfileName);
        if(nameExist) throw new BadHttpRequestException("Já existe um perfil com este nome");

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

    public async Task<ListUserProfileDto> GetUserProfileByIdAsync(Guid userId)
    {
        var user = await _appDbContext.Users
            .Include(u => u.Profiles)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException("Perfil não encontrado");
        }

        return new ListUserProfileDto
        {
            UserId = user.Id,
            UserName = user.Name,
            Profiles = user.Profiles?.Select(p => p.ProfileName).ToArray()
        };

    }

    public async Task<EditProfileResponse> EditProfileByIdAsync(Guid id, EditProfileDto editProfileDto)
    {
        var response = new EditProfileResponse();
        var existingProfile = await _appDbContext.Profiles.FindAsync(id);
        if (existingProfile == null)
        {
            response.Message = "Perfil não encontrado";
            return response;
        }
    

        if (existingProfile.ProfileName == editProfileDto.ProfileName)
        {

            var existingProfileWithSameName = await _appDbContext.Profiles
                .FirstOrDefaultAsync(u => u.ProfileName == editProfileDto.ProfileName);
        
            if (existingProfileWithSameName != null)
            {
                response.Message = "Já tem um perfil com este nome";
                return response;
            }
        }
        
        existingProfile.ProfileName = editProfileDto.ProfileName;

        await _appDbContext.SaveChangesAsync();

        response.Message = "Perfil Atualizado com sucesso";
        return response;
    }

    public async Task<EditProfileResponse> ListProfileByIdAsync(Guid id)
    {
        var existingProfile = await _appDbContext.Profiles.FindAsync(id);
        if (existingProfile == null)
        {
            return new EditProfileResponse
            {
                Message = "Perfil não encontrado"
            };
        }

        return new EditProfileResponse
        {
            Profilename = existingProfile.ProfileName,
            Message = "Perfil encontrado!"
        };
    }
}