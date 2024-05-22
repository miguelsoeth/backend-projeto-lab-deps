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
    private readonly IUserService _userService;
    private readonly AppDbContext _appDbContext;

    public AccountController(IUserService userService, AppDbContext appDbContext)
    {
        _userService = userService;
        _appDbContext = appDbContext;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LogUserIn(LoginDto loginDto)
    {
        var result = await _userService.LoginUserAsync(loginDto);
        return Ok(result);
    }
    
    [HttpPost("register")] 
    public async Task<ActionResult<LoginResponse>> RegisterUser(RegisterUserDto registerUser)
    {
        var result = await _userService.RegisterUserAsync(registerUser);
        return Ok(result);
    }

    [HttpPut("EditUser/{id}")]
    public async Task<ActionResult> EditUser([FromRoute] string id, EditDto editDto)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return BadRequest("User not found");
        }

        if (!string.IsNullOrEmpty(editDto.Password))
        {
            editDto.Password = BCrypt.Net.BCrypt.HashPassword(editDto.Password);
            user.Password = editDto.Password;
        }

        user.Name = editDto.Name;
        user.Email = editDto.Email;
        user.IsActive = editDto.IsActive;
        user.Roles = editDto.Roles;

        _appDbContext.Users.Update(user);    
        await _appDbContext.SaveChangesAsync();
        return Ok("Usuário editado com sucesso!");
    }
    
    

    [HttpGet("AllUsers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AllUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return Ok(result);
    }

    [HttpGet("UserId")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetUserById(string id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(result);
    }
    
    [HttpGet("CurrentUser")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApplicationUser>> GetCurrentUser()
    {
        var result = await _userService.GetCurrentLoggedInUserAsync(HttpContext);

        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
}