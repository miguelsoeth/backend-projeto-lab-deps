using Application.Dtos;

namespace Application.Contract;

public interface IUser
{
    Task<RegistrationResponse> RegisterUserAsync(RegisterUserDto registerUserDto);
    Task<LoginResponse> LoginUserAsync(LoginDto loginDto);
}