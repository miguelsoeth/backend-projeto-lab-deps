using Application.Dtos.Account;
using Microsoft.AspNetCore.Http;


namespace Application.Contract;

public interface IUserRepository
{
    Task<AuthResponseDto> RegisterUserAsync(UserDetailDto registerUser);
    Task<AuthResponseDto> LoginUserAsync(LoginDto loginDto);
    Task<AuthResponseDto> EditUserAsync(string id, UserDetailDto editUserDto);
    Task<List<UserDetailDto>> GetAllUsersAsync();
    Task<UserDetailDto> GetUserByIdAsync(Guid id);
    Task<UserDetailDto?> GetCurrentLoggedInUserAsync(HttpContext context);
    Task<AuthResponseDto> RefreshToken(TokenDto tokenDto);
}