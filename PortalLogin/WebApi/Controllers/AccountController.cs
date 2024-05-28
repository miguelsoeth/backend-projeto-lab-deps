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
    private readonly IUserService _userService;
    private readonly AppDbContext _appDbContext;

    public AccountController(IUserService userService, AppDbContext appDbContext)
    {
        _userService = userService;
        _appDbContext = appDbContext;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> LogUserIn(LoginDto loginDto)
    {
        var result = await _userService.LoginUserAsync(loginDto);
        return Ok(result);
    }
    
    [HttpPost("register")]
    //[Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuthResponseDto>> RegisterUser(UserDetailDto registerUser)
    {
        var result = await _userService.RegisterUserAsync(registerUser);
        return Ok(result);
    }

    [HttpPut("edit/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuthResponseDto>> EditUser([FromRoute] string id, UserDetailDto editUserDto)
    {
        var result = await _userService.EditUserAsync(id, editUserDto);
        return Ok(result);
    }
    
    [HttpGet("account/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetUserById(string id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        if (result == null) return NotFound(result);
        return Ok(result);
    }
    
    [HttpGet("account/all-users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDetailDto>>> AllUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return Ok(result);
    }
    
    [HttpGet("account/current-user")]
    [Authorize]
    public async Task<ActionResult<UserDetailDto>> GetCurrentUser()
    {
        var result = await _userService.GetCurrentLoggedInUserAsync(HttpContext);
        return Ok(result);
    }
    
}