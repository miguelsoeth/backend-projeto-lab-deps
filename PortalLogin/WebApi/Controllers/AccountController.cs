using Application.Contract;
using Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IUser _user;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(IUser user, RoleManager<IdentityRole> roleManager)
    {
        _user = user;
        _roleManager = roleManager;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LogUserIn(LoginDto loginDto)
    {
        var result = await _user.LoginUserAsync(loginDto);
        return Ok(result);
    }
    
    [HttpPost("register")] 
    public async Task<ActionResult<LoginResponse>> RegisterUser(RegisterUserDto registerUser)
    {
        var result = await _user.RegisterUserAsync(registerUser);
        return Ok(result);
    }
    
}