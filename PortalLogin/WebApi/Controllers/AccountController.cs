using Application.Contract;
using Application.Dtos;
using Application.Dtos.Account;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AccountController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> LogUserIn(LoginDto loginDto)
    {
        var result = await _userRepository.LoginUserAsync(loginDto);
        if (result.IsSuccess == false)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuthResponseDto>> RegisterUser(UserDetailDto registerUser)
    {
        var result = await _userRepository.RegisterUserAsync(registerUser);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuthResponseDto>> EditUser([FromRoute] string id, UserDetailDto editUserDto)
    {
        var result = await _userRepository.EditUserAsync(id, editUserDto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetUserById(Guid id)
    {
        var result = await _userRepository.GetUserByIdAsync(id);
        if (result == null) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("all-users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDetailDto>>> AllUsers()
    {
        var result = await _userRepository.GetAllUsersAsync();
        return Ok(result);
    }

    [HttpGet("current-user")]
    [Authorize]
    public async Task<ActionResult<UserDetailDto>> GetCurrentUser()
    {
        var result = await _userRepository.GetCurrentLoggedInUserAsync(HttpContext);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult> refreshToken(TokenDto tokenDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userRepository.RefreshToken(tokenDto);
        if (result.IsSuccess == false)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}