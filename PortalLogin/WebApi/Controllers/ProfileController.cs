using System.Security.Claims;
using Application.Contract;
using Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("CreateProfile")]
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
}