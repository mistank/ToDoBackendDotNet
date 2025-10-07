using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoBackend.Application.DTOs;
using ToDoBackend.Application.Services;

namespace ToDoBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers([FromQuery] int skip = 0, [FromQuery] int limit = 100)
    {
        try
        {
            var users = await _userService.GetAllAsync(skip, limit);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { detail = $"Internal server error: {ex.Message}" });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { detail = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { detail = $"Internal server error: {ex.Message}" });
        }
    }

    [HttpPut("{username}")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(string username, [FromBody] UserUpdateDto userDto)
    {
        try
        {
            var user = await _userService.UpdateAsync(username, userDto);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { detail = $"Internal server error: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> DeleteUser(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);
            return Ok(new { detail = "User deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { detail = $"Internal server error: {ex.Message}" });
        }
    }
}
