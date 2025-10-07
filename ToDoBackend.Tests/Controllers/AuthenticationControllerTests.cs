using Microsoft.AspNetCore.Mvc;
using Moq;
using ToDoBackend.API.Controllers;
using ToDoBackend.Application.DTOs;
using ToDoBackend.Application.Services;
using FluentAssertions;

namespace ToDoBackend.Tests.Controllers;

public class AuthenticationControllerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        _controller = new AuthenticationController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithUser()
    {
        var userCreateDto = new UserCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "password123",
            PermissionId = 2
        };

        var expectedResponse = new UserResponseDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            IsActive = 1,
            PermissionId = 2
        };

        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<UserCreateDto>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Register(userCreateDto);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        var userCreateDto = new UserCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "existing@example.com",
            Password = "password123",
            PermissionId = 2
        };

        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<UserCreateDto>()))
            .ThrowsAsync(new InvalidOperationException("Email already registered"));

        var result = await _controller.Register(userCreateDto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        var username = "johndoe";
        var password = "password123";

        var expectedResponse = new LoginResponseDto
        {
            AccessToken = "sample.jwt.token",
            TokenType = "bearer"
        };

        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Login(username, password);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var username = "johndoe";
        var password = "wrongpassword";

        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ThrowsAsync(new UnauthorizedAccessException("Incorrect username or password"));

        var result = await _controller.Login(username, password);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithException_ReturnsInternalServerError()
    {
        var username = "johndoe";
        var password = "password123";

        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _controller.Login(username, password);

        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Register_WithException_ReturnsInternalServerError()
    {
        var userCreateDto = new UserCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "password123",
            PermissionId = 2
        };

        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<UserCreateDto>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _controller.Register(userCreateDto);

        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
