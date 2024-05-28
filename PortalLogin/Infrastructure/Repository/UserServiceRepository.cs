
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Contract;
using Application.Dtos;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

    public async Task<AuthResponseDto> RegisterUserAsync(UserDetailDto registerUser)
    {
        var getUser = await FindUserByEmail(registerUser.Email!);
        //Verifica se o usuário já existe
        if (getUser != null) return new AuthResponseDto
        {
            IsSuccess = false,
            Message = "Usuário já existe."
        };
        //Verifica se os campos necessários não são nulos
        string? reason = null;
        if (registerUser.Password.IsNullOrEmpty()) reason = "Obrigatório preencher campo Senha!";
        if (registerUser.Name.IsNullOrEmpty()) reason = "Obrigatório preencher campo Nome!";
        if (reason != null) return new AuthResponseDto
        {
            IsSuccess = false,
            Message = reason
        };
        //Verifica de as roles estão vazias e corrige
        if (registerUser.Roles.IsNullOrEmpty())
        {
            registerUser.Roles = new List<string>
            {
                "User"
            };
        }
        //Adiciona o usuário no banco de dados
        _appDbContext.Users.Add(new ApplicationUser()
        {
            Name = registerUser.Name,
            Email = registerUser.Email,
            Document = registerUser.Document,
            Password = BCrypt.Net.BCrypt.HashPassword(registerUser.Password),
            Roles = registerUser.Roles,
            IsActive = true
        });
        await _appDbContext.SaveChangesAsync();
        
        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Registro completado com sucesso!"
        };
    }

    public async Task<AuthResponseDto> LoginUserAsync(LoginDto loginDto)
    {
        var getUser = await FindUserByEmail(loginDto.Email!);

        if (getUser == null!) return new AuthResponseDto
        {
            IsSuccess = false,
            Message = "Usuário não encontrado."
        };
        
        if (getUser.IsActive == false) return new AuthResponseDto
        {
            IsSuccess = false,
            Message = "Usuário desativado."
        };
        
        bool checkPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, getUser.Password);
        if (!checkPassword) return new AuthResponseDto
        {
            IsSuccess = false,
            Message = "Senha incorreta."
        };

        var refreshToken = GenerateRefreshToken();
        _ = int.TryParse(_configuration.GetSection("Jwt").GetSection("RefreshTokenValidityIn").Value!, out int RefreshTokenValidityIn);
        getUser.RefreshToken = refreshToken;
        getUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(RefreshTokenValidityIn);
        await _appDbContext.SaveChangesAsync();
        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Logado com sucesso!",
            Token = GenerateJwtToken(getUser),
            RefreshToken = refreshToken
        };
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
    
    
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    
    
    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt").GetSection("Key").Value!)),
            ValidateLifetime = false


        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Token Inválido!");

        return principal;
    }

    public async Task<AuthResponseDto> RefreshToken(TokenDto tokenDto)
    {
        var principal = GetPrincipalFromExpiredToken(tokenDto.Token);
        var user = await FindUserByEmail(tokenDto.Email);

        if (principal is null || user is null || user.RefreshToken != tokenDto.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Solicitação de cliente inválida!"
            };
        }
        
        var newJwtToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        _ = int.TryParse(_configuration.GetSection("Jwt").GetSection("RefreshTokenValidityIn").Value!, out int RefreshTokenValidityIn);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(RefreshTokenValidityIn);

        await _appDbContext.SaveChangesAsync(); 

        return new AuthResponseDto
        {
            IsSuccess = true,
            Token = newJwtToken,
            RefreshToken = newRefreshToken,
            Message = "Token atualizado com sucesso!"
        };

    }
    
}