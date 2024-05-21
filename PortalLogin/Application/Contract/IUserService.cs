using Application.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Contract;

public interface IUserService
{
    Task<RegistrationResponse> RegisterUserAsync(RegisterUserDto registerUserDto);
    Task<LoginResponse> LoginUserAsync(LoginDto loginDto);
    Task<List<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser> GetUserByIdAsync(string id);
    Task<ApplicationUser?> GetCurrentLoggedInUserAsync(HttpContext context);
}