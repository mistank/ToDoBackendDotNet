using Microsoft.AspNetCore.Mvc;
using ToDoBackend.Application.DTOs;
using ToDoBackend.Application.Services;

namespace ToDoBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthenticationController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] UserCreateDto userDto)
    {
        try
        {
            var user = await _authService.RegisterAsync(userDto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { detail = $"Internal server error: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromForm] string username, [FromForm] string password)
    {
        try
        {
            var loginDto = new LoginRequestDto
            {
                Username = username,
                Password = password
            };

            var response = await _authService.LoginAsync(loginDto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { detail = $"Internal server error: {ex.Message}" });
        }
    }
}
