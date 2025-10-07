using ToDoBackend.Application.DTOs;

namespace ToDoBackend.Application.Services;

public interface IUserService
{
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<UserResponseDto?> GetByUsernameAsync(string username);
    Task<IEnumerable<UserResponseDto>> GetAllAsync(int skip = 0, int limit = 100);
    Task<UserResponseDto> CreateAsync(UserCreateDto userDto);
    Task<UserResponseDto> UpdateAsync(string username, UserUpdateDto userDto);
    Task DeleteAsync(int id);
}
