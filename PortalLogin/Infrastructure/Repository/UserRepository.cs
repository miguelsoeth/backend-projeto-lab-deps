
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Contract;
using Application.Dtos;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Repository;

public class UserRepository : IUser
{
    private readonly AppDbContext _appDbContext;
    private readonly IConfiguration _configuration;

    public UserRepository(AppDbContext appDbContext, IConfiguration configuration)
    {
        _appDbContext = appDbContext;
        _configuration = configuration;
    }
    
    private async Task<ApplicationUser> FindUserByEmail(string email) =>
        await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<RegistrationResponse> RegisterUserAsync(RegisterUserDto registerUserDto)
    {
        var getUser = await FindUserByEmail(registerUserDto.Email!);
        if (getUser != null)
            return new RegistrationResponse(false, "User already exist!");

        _appDbContext.Users.Add(new ApplicationUser()
        {
            Name = registerUserDto.Name,
            Email = registerUserDto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password),
            Roles = registerUserDto.Roles
        });
        await _appDbContext.SaveChangesAsync();
        return new RegistrationResponse(true, "Registration completed"); 
    }

    public async Task<LoginResponse> LoginUserAsync(LoginDto loginDto)
    {
        var getUser = await FindUserByEmail(loginDto.Email!);

        if (getUser == null!) return new LoginResponse(false, "User not found");

        bool checkPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, getUser.Password);
        if (checkPassword)
            return new LoginResponse(true, "Login successfully", GenerateJwtToken(getUser));
        else
            return new LoginResponse(false, "Invalid credentials");
    }

    public string GenerateJwtToken(ApplicationUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        // Adiciona as roles como claims
        if (user.Roles != null)
        {
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}