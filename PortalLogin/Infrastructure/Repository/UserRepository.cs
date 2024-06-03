
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Contract;
using Application.Dtos;
using Application.Dtos.Account;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserRepository(AppDbContext appDbContext, IConfiguration configuration,
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

    public async Task<AuthResponseDto> EditUserAsync(string id, UserDetailDto editUserDto)
    {
        //Verifica o Id
        if (!Guid.TryParse(id, out Guid userId))
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Id inválido!"
            }; 
        }
        //Encontra o usuário
        var user = await _appDbContext.Users.FindAsync(userId);
        if (user == null!) return new AuthResponseDto
        {
            IsSuccess = false,
            Message = "Usuário não encontrado."
        };
        //Verifica campos e edita o usuário
        user.IsActive = editUserDto.IsActive;
        user.Roles = editUserDto.Roles;
        
        if (!string.IsNullOrEmpty(editUserDto.Name)) user.Name = editUserDto.Name;
        
        if (!string.IsNullOrEmpty(editUserDto.Document)) user.Document = editUserDto.Document;
        
        if (!string.IsNullOrEmpty(editUserDto.Password))
        {
            editUserDto.Password = BCrypt.Net.BCrypt.HashPassword(editUserDto.Password);
            user.Password = editUserDto.Password;
        }
        
        if (!string.IsNullOrEmpty(editUserDto.Email))
        {
            var emailExists = await _appDbContext.Users.AnyAsync(u => u.Email == editUserDto.Email);
            if (emailExists) return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Email em uso!"
            };
            user.Email = editUserDto.Email;
        }
        //Insere o usuário no banco
        _appDbContext.Users.Update(user);
        await _appDbContext.SaveChangesAsync();
        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Usuário editado com sucesso!"
        };
    }
    
    public async Task<List<UserDetailDto>> GetAllUsersAsync()
    {
        var users = await _appDbContext.Users.Select(u => new UserDetailDto
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            Document = u.Document,
            Roles = u.Roles,
            IsActive = u.IsActive
        }).ToListAsync();
        return users;
    }

    public async Task<UserDetailDto> GetUserByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out Guid userId))
        {
            return null;
        }
        var user = await _appDbContext.Users.FindAsync(userId);
        if (user is null) return null;
        
        return new UserDetailDto() {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Document = user.Document,
            Roles = user.Roles,
            IsActive = user.IsActive
        };
    }

    public async Task<UserDetailDto?> GetCurrentLoggedInUserAsync(HttpContext context)
    {
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };

        var userEmailClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email);
        var userNameClaim = _contextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Name);

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
                return new UserDetailDto() {
                    Id = deserializedUser!.Id,
                    Email = deserializedUser.Email,
                    Name = deserializedUser.Name,
                    Document = deserializedUser.Document,
                    Roles = deserializedUser.Roles,
                    IsActive = deserializedUser.IsActive
                };
            }
        }

        return null;
    }
    /*
    public string GenerateJwtToken(ApplicationUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
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
    */
    public string GenerateJwtToken(ApplicationUser user){
        var tokenHandler = new JwtSecurityTokenHandler();
            
        var key = Encoding.ASCII
            .GetBytes(_configuration.GetSection("Jwt").GetSection("Key").Value!);

        List<Claim> claims = 
        [
            new (JwtRegisteredClaimNames.Email,user.Email??""),
            new (JwtRegisteredClaimNames.Name,user.Name??""),
            new (JwtRegisteredClaimNames.NameId,user.Id.ToString()),
            new (JwtRegisteredClaimNames.Aud,
                _configuration.GetSection("Jwt").GetSection("Audience").Value!),
            new (JwtRegisteredClaimNames.Iss,_configuration.GetSection("Jwt").GetSection("Issuer").Value!)
        ];


        foreach(var role in user.Roles!)

        {
            claims.Add(new Claim(ClaimTypes.Role,role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        };

        var token  = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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