using System.Security.Claims;
using Application.Contract;
using Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;

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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProfileResponse>> CreateProfile([FromBody] ProfileDto profileDto)
    {
        var userId = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User is not Authenticated");
        }

        var profileResponse = await _profile.CreateProfileAsync(userId, profileDto);
        return Ok(profileResponse);

    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetProfileById(Guid id, [FromBody] EditProfileDto editProfileDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _profile.EditProfileByIdAsync(id, editProfileDto);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProfileById(Guid id)
    {
        var result = await _profile.DeleteProfileByIdAsync(id);
        return Ok(result);

    }
    
    [HttpGet("{userId:guid}/profiles")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetUserProfile(Guid userId)
    {
        try
        {
            var result = await _profile.GetUserProfileByIdAsync(userId);
            return Ok(result.Profiles);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}