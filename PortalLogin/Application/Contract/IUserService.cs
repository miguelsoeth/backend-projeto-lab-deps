using Application.Dtos;
using Application.Dtos.Account;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Contract;

public interface IUserService
{
    Task<AuthResponseDto> RegisterUserAsync(UserDetailDto registerUser);
    Task<AuthResponseDto> LoginUserAsync(LoginDto loginDto);
    Task<AuthResponseDto> EditUserAsync(string id, UserDetailDto editUserDto);
    Task<List<UserDetailDto>> GetAllUsersAsync();
    Task<UserDetailDto> GetUserByIdAsync(string id);
    Task<UserDetailDto?> GetCurrentLoggedInUserAsync(HttpContext context);
    Task<AuthResponseDto> RefreshToken(TokenDto tokenDto);
}