using Microsoft.AspNetCore.Mvc;
using Moq;
using ToDoBackend.API.Controllers;
using ToDoBackend.Application.DTOs;
using ToDoBackend.Application.Services;
using FluentAssertions;

namespace ToDoBackend.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithUserList()
    {
        var expectedUsers = new List<UserResponseDto>
        {
            new UserResponseDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                IsActive = 1,
                PermissionId = 2
            },
            new UserResponseDto
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Username = "janesmith",
                Email = "jane@example.com",
                IsActive = 1,
                PermissionId = 2
            }
        };

        _userServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedUsers);

        var result = await _controller.GetUsers(0, 100);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task GetUsers_WithCustomPagination_ReturnsOkWithUserList()
    {
        var skip = 10;
        var limit = 50;
        var expectedUsers = new List<UserResponseDto>();

        _userServiceMock
            .Setup(s => s.GetAllAsync(skip, limit))
            .ReturnsAsync(expectedUsers);

        var result = await _controller.GetUsers(skip, limit);

        result.Result.Should().BeOfType<OkObjectResult>();
        _userServiceMock.Verify(s => s.GetAllAsync(skip, limit), Times.Once);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsOkWithUser()
    {
        var userId = 1;
        var expectedUser = new UserResponseDto
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            IsActive = 1,
            PermissionId = 2
        };

        _userServiceMock
            .Setup(s => s.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        var result = await _controller.GetUser(userId);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        var userId = 999;

        _userServiceMock
            .Setup(s => s.GetByIdAsync(userId))
            .ReturnsAsync((UserResponseDto?)null);

        var result = await _controller.GetUser(userId);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsOkWithUpdatedUser()
    {
        var username = "johndoe";
        var userUpdateDto = new UserUpdateDto
        {
            FirstName = "John",
            LastName = "Updated",
            Email = "johnupdated@example.com",
            IsActive = 1
        };

        var expectedUser = new UserResponseDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Updated",
            Username = username,
            Email = "johnupdated@example.com",
            IsActive = 1,
            PermissionId = 2
        };

        _userServiceMock
            .Setup(s => s.UpdateAsync(username, It.IsAny<UserUpdateDto>()))
            .ReturnsAsync(expectedUser);

        var result = await _controller.UpdateUser(username, userUpdateDto);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentUser_ReturnsNotFound()
    {
        var username = "nonexistent";
        var userUpdateDto = new UserUpdateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            IsActive = 1
        };

        _userServiceMock
            .Setup(s => s.UpdateAsync(username, It.IsAny<UserUpdateDto>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var result = await _controller.UpdateUser(username, userUpdateDto);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ReturnsOk()
    {
        var userId = 1;

        _userServiceMock
            .Setup(s => s.DeleteAsync(userId))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteUser(userId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentId_ReturnsNotFound()
    {
        var userId = 999;

        _userServiceMock
            .Setup(s => s.DeleteAsync(userId))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var result = await _controller.DeleteUser(userId);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUsers_WithException_ReturnsInternalServerError()
    {
        _userServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _controller.GetUsers(0, 100);

        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetUser_WithException_ReturnsInternalServerError()
    {
        var userId = 1;

        _userServiceMock
            .Setup(s => s.GetByIdAsync(userId))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _controller.GetUser(userId);

        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateUser_WithException_ReturnsInternalServerError()
    {
        var username = "johndoe";
        var userUpdateDto = new UserUpdateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            IsActive = 1
        };

        _userServiceMock
            .Setup(s => s.UpdateAsync(username, It.IsAny<UserUpdateDto>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _controller.UpdateUser(username, userUpdateDto);

        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteUser_WithException_ReturnsInternalServerError()
    {
        var userId = 1;

        _userServiceMock
            .Setup(s => s.DeleteAsync(userId))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _controller.DeleteUser(userId);

        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
