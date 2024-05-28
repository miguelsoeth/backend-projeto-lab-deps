using Application.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Contract;

public interface IUserService
{
    Task<AuthResponseDto> RegisterUserAsync(UserDetailDto registerUser);
    Task<AuthResponseDto> LoginUserAsync(LoginDto loginDto);
    Task<List<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser> GetUserByIdAsync(string id);
    Task<ApplicationUser?> GetCurrentLoggedInUserAsync(HttpContext context);
    Task<AuthResponseDto> RefreshToken(TokenDto tokenDto);
}