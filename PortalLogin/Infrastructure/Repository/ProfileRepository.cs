using System.Security.Claims;
using Application.Contract;
using Application.Dtos;
using Application.Dtos.Profile;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository;

public class ProfileRepository : IProfileRepository
{

    private readonly AppDbContext _appDbContext;
    private readonly IHttpContextAccessor _contextAccessor;

    public ProfileRepository(IHttpContextAccessor contextAccessor, AppDbContext appDbContext)
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
        if (nameExist) return new ProfileResponse
        {
            Message = "Já existe um perfil com esse nome!"
        };

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
            Message = "Usuário criado com sucesso!"
        };
    }

    public async Task<List<ProfileResponse>> GetUserProfilesByIdAsync(Guid userId)
    {
        var user = await _appDbContext.Users
            .Include(u => u.Profiles)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado.");
        }

        var res = user.Profiles!.Select(
            p => new ProfileResponse
            {
                Id = p.Id, 
                UserId = p.UserId, 
                ProfileName = p.ProfileName!
            }
        ).ToList();

        return res;

    }

    public async Task<ProfileResponse> EditProfileByIdAsync(Guid id, ProfileDto editProfileDto)
    {
        var existingProfile = await _appDbContext.Profiles.FindAsync(id);
        if (existingProfile == null)
        {
            return new ProfileResponse
            {
                Message = "Perfil não encontrado"
            };
        }
    

        if (existingProfile.ProfileName == editProfileDto.ProfileName)
        {

            var existingProfileWithSameName = await _appDbContext.Profiles
                .FirstOrDefaultAsync(u => u.ProfileName == editProfileDto.ProfileName);
        
            if (existingProfileWithSameName != null)
            {
                return new ProfileResponse
                {
                    Message = "Já existe um perfil com este nome"
                };
            }
        }
        
        existingProfile.ProfileName = editProfileDto.ProfileName;

        await _appDbContext.SaveChangesAsync();

        return new ProfileResponse
        {
            Id = existingProfile.Id,
            UserId = existingProfile.UserId,
            ProfileName = editProfileDto.ProfileName,
            Message = "Perfil Atualizado com sucesso"
        };
    }

    #region LIST PROFILES BY ID (NOT USED)
    /*
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
    */
    

    #endregion
    
    public async Task<ProfileResponse> DeleteProfileByIdAsync(Guid id)
    {
        var existingProfile = await _appDbContext.Profiles.FindAsync(id);
        if (existingProfile == null)
        {
            return new ProfileResponse
            {
                Message = "Perfil não encontrado"
            };
        }

        _appDbContext.Profiles.Remove(existingProfile);
        await _appDbContext.SaveChangesAsync();

        return new ProfileResponse
        {
            Id = existingProfile.Id,
            ProfileName = existingProfile.ProfileName!,
            UserId = existingProfile.UserId,
            Message = "Perfil deletado com sucesso"
        };
    }
}