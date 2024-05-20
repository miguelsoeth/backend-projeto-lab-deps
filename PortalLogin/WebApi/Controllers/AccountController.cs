using Application.Contract;
using Application.Dtos;
using Domain.Entities;
using Infrastructure.Data;
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
    private readonly AppDbContext _appDbContext;


    public AccountController(IUser user, AppDbContext appDbContext)
    {
        _user = user;
        _appDbContext = appDbContext;
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
    
}