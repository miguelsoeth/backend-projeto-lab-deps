using System.Security.Claims;
using Application.Contract;
using Application.Dtos;
using Application.Dtos.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profile;
    private readonly IHttpContextAccessor _contextAccessor;

    public ProfileController(IProfileService profile, IHttpContextAccessor contextAccessor)
    {
        _profile = profile;
        _contextAccessor = contextAccessor;
    }

    [HttpPost("create/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProfileResponse>> CreateProfile([FromBody] ProfileDto profileDto, Guid userId)
    {
        var profileResponse = await _profile.CreateProfileAsync(userId, profileDto);
        
        if (profileResponse.ProfileName.IsNullOrEmpty()) 
            return Conflict(profileResponse);
        
        return Ok(profileResponse);

    }

    [HttpPut("edit/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> EditProfileById(Guid id, [FromBody] ProfileDto editProfileDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _profile.EditProfileByIdAsync(id, editProfileDto);
        
        if (result.ProfileName.IsNullOrEmpty()) 
            return Conflict(result);
        
        return Ok(result);
    }

    [HttpDelete("delete/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProfileById(Guid id)
    {
        var result = await _profile.DeleteProfileByIdAsync(id);
        
        if (result.ProfileName.IsNullOrEmpty()) 
            return NotFound(result);
        
        return Ok(result);

    }
    
    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetUserProfiles(Guid userId)
    {
        try
        {
            var result = await _profile.GetUserProfilesByIdAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}