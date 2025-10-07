using ToDoBackend.Application.DTOs;

namespace ToDoBackend.Application.Services;

public interface IAuthenticationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);
    Task<UserResponseDto> RegisterAsync(UserCreateDto userDto);
}
