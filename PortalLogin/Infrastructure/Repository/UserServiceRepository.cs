
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Contract;
using Application.Dtos;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Repository;

public class UserServiceRepository : IUserService
{
    private readonly AppDbContext _appDbContext;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserServiceRepository(AppDbContext appDbContext, IConfiguration configuration,
        IHttpContextAccessor contextAccessor)
    {
        _appDbContext = appDbContext;
        _configuration = configuration;
        _contextAccessor = contextAccessor;
    }
    
    private async Task<ApplicationUser> FindUserByEmail(string email) =>
        await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<RegistrationResponse> RegisterUserAsync(RegisterUserDto registerUserDto)
    {
        var getUser = await FindUserByEmail(registerUserDto.Email!);
        if (getUser != null)
            return new RegistrationResponse(false, "Usuário já existe");

        _appDbContext.Users.Add(new ApplicationUser()
        {
            Name = registerUserDto.Name,
            Email = registerUserDto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password),
            Roles = registerUserDto.Roles,
            IsActive = true
        });
        await _appDbContext.SaveChangesAsync();
        return new RegistrationResponse(true, "Registration completed"); 
    }

    public async Task<LoginResponse> LoginUserAsync(LoginDto loginDto)
    {
        var getUser = await FindUserByEmail(loginDto.Email!);
        var isActive = await _appDbContext.Users.FirstOrDefaultAsync(u => u.IsActive != true);

        if (getUser.IsActive == false)
        {
            return new LoginResponse(false, "Usuário desativado");
        }
        
        if (getUser == null!) return new LoginResponse(false, "User not found");

        bool checkPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, getUser.Password);
        if (checkPassword)
            return new LoginResponse(true, "Login successfully", GenerateJwtToken(getUser));
        else
            return new LoginResponse(false, "Invalid credentials");
    }


    public async Task<List<ApplicationUser>> GetAllUsersAsync()
    {
        return await _appDbContext.Users.ToListAsync();
    }

    public async Task<ApplicationUser> GetUserByIdAsync(string id)
    {
        return (await _appDbContext.Users.FindAsync(Guid.Parse(id)))!;
    }

    public async Task<ApplicationUser?> GetCurrentLoggedInUserAsync(HttpContext context)
    {
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };

        var userEmailClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email);
        var userNameClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name);

        if (userEmailClaim != null && userNameClaim != null)
        {
            var userEmail = userEmailClaim.Value;
            var userName = userNameClaim.Value;
        
            // Carrega o usuário com os perfis associados
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null)
            {
                user.Name = userName;

                // Serializa e desserializa o usuário para aplicar o ReferenceHandler
                var serializedUser = JsonSerializer.Serialize(user, options);
                var deserializedUser = JsonSerializer.Deserialize<ApplicationUser>(serializedUser, options);

                return deserializedUser;
            }
        }

        return null;
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
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}