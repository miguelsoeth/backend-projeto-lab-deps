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

    public async Task<ProfileResponse> CreateProfileAsync(Guid userId, ProfileDto profileDto)
    {

        var user = await _appDbContext.Users.FindAsync(userId);

        if (user == null)
        {
            return new ProfileResponse
            {
                Message = "Usuário não encontrado!"
            };
        }

        var nameExist = await _appDbContext.Profiles.AnyAsync(u => u.ProfileName == profileDto.ProfileName && u.UserId == userId );
        if (nameExist) return new ProfileResponse
        {
            Message = "Esse usuário já possui um perfil com esse nome!"
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
        var profile = await _appDbContext.Profiles.FindAsync(id);
        if (profile == null)
        {
            return new ProfileResponse
            {
                Message = "Perfil não encontrado"
            };
        }

        if (profile.ProfileName == editProfileDto.ProfileName)
        {
            return new ProfileResponse
            {
                Message = "Nome não alterado!"
            };
        }
        
        var sameNameProfile = await _appDbContext.Profiles
            .FirstOrDefaultAsync(u => u.ProfileName == editProfileDto.ProfileName && u.UserId == Guid.Parse(editProfileDto.userId));
        
        if (sameNameProfile != null)
        {
            return new ProfileResponse
            {
                Message = "Já existe um perfil com este nome"
            };
        }
        
        profile.ProfileName = editProfileDto.ProfileName;

        await _appDbContext.SaveChangesAsync();

        return new ProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
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