using Application.Contract;
using Application.Dtos;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IUser _user;
    private readonly UserRepository _repository;

    public AccountController(IUser user)
    {
        _user = user;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LogUserIn(LoginDto loginDto)
    {
        var result = await _user.LoginUserAsync(loginDto);
        return Ok(result);
    }
    
    [HttpPost("register")] 
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LoginResponse>> RegisterUser(RegisterUserDto registerUser)
    {
        var result = await _user.RegisterUserAsync(registerUser);
        return Ok(result);
    }

    [HttpGet("AllUsers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AllUsers()
    {
        var result = await _user.GetAllUsersAsync();
        return Ok(result);
    }

    [HttpGet("UserId")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetUserById(int id)
    {
        var result = await _user.GetUserByIdAsync(id);
        return Ok(result);
    }
    
    [HttpGet("CurrentUser")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApplicationUser>> GetCurrentUser()
    {
        var result = await _user.GetCurrentLoggedInUserAsync(HttpContext);

        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
}