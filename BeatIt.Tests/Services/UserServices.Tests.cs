using BeatIt.Errors;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Repositories;
using BeatIt.Services.PasswordService;
using BeatIt.Services.UserService;
using BeatIt.Tests.Models;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace BeatIt.Tests.Services;

public class UserServicesTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IPasswordService> _hasher;
    private readonly UserService _userService;
    private readonly ITestOutputHelper _output;

    public UserServicesTests(ITestOutputHelper output)
    {
        _output = output;
        _userRepository = new Mock<IUserRepository>();
        _hasher = new Mock<IPasswordService>();
        _userService = new UserService(_userRepository.Object, _hasher.Object);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnSuccess()
    {
        // Arrange
        var newUser = new UserCreateDto
        {
            Email = "test@example.com",
            Password = "password",
            Name = "Teste",
        };

        _userRepository
            .Setup(repo => repo.GetByEmail(newUser.Email))
            .ReturnsAsync((User?)null);

        var salt = new byte[16];
        _hasher
            .Setup(hasher => hasher.GenerateSalt())
            .Returns(salt);
        _hasher.Setup(hasher => hasher.HashPassword(It.IsAny<string>(), It.IsAny<byte[]>()))
               .Callback<string, byte[]>((password, salt) =>
               {
                   Assert.Equal("password", password);
                   Assert.NotNull(salt);
                   Assert.True(salt.Length > 0);
               })
               .Returns("hashed_password");

        _userRepository
            .Setup(repo => repo.Add(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _userService.CreateUser(newUser);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(newUser.Name, result.Value.Name);
        Assert.Equal(newUser.Email, result.Value.Email);
        _userRepository.Verify(repo => repo.GetByEmail(It.IsAny<string>()), Times.Once);
        _hasher.Verify(hasher => hasher.GenerateSalt(), Times.Once);
        _hasher.Verify(hasher => hasher.HashPassword(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        _userRepository.Verify(repo => repo.Add(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var newUser = new UserCreateDto
        {
            Email = "test@mail.com",
            Password = "password",
            Name = "Teste",
        };

        _userRepository
            .Setup(repo => repo.GetByEmail(newUser.Email))
            .ReturnsAsync(new User());
        // Act
        var result = await _userService.CreateUser(newUser);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.AlreadyExists, result.Error);
        _userRepository.Verify(repo => repo.GetByEmail(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnSuccess()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Created_at = user.Created_at,
            Updated_at = user.Updated_at,
            IsActive = user.IsActive,
        };
        _userRepository
            .Setup(repo => repo.GetById(user.Id))
            .ReturnsAsync(user);
        var result = await _userService.GetUserById(user.Id);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(userDto.Email, result.Value.Email);
        _userRepository.Verify(repo => repo.GetById(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetUserById_WhenUserNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var user = Guid.NewGuid();

        _userRepository
            .Setup(repo => repo.GetById(user))
            .ReturnsAsync((User)null!);
        // Act
        var result = await _userService.GetUserById(user);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.UserNotFound, result.Error);
        _userRepository.Verify(repo => repo.GetById(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnListUsers()
    {
        // Arrange
        var users = TestData.GetValidUsers();

        _userRepository
            .Setup(repo => repo.GetAllUsers())
            .ReturnsAsync(users);
        // Act
        var result = await _userService.GetUsers();
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(users.Count, result.Value.Count);
        _userRepository.Verify(repo => repo.GetAllUsers(), Times.Once);
    }

    [Fact]
    public async Task SoftDelete_ShouldReturnSuccess()
    {
        // Arrange
        var user = TestData.GetValidUser();
        _userRepository
            .Setup(repo => repo.GetById(user.Id))
            .ReturnsAsync(user);
        // Act
        var result = await _userService.SoftDelete(user.Id);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("User deleted Successfully", result.Value);
        Assert.False(user.IsActive);
        _userRepository.Verify(repo => repo.GetById(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task SoftDelete_WhenUserNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var user = Guid.NewGuid();

        _userRepository
            .Setup(repo => repo.GetById(user))
            .ReturnsAsync((User)null!);
        // Act        
        var result = await _userService.SoftDelete(user);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.UserNotFound, result.Error);
        _userRepository.Verify(repo => repo.GetById(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnSuccess()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var updateUser = new UserUpdateDto { Name = "Updated Name" };

        _userRepository
            .Setup(repo => repo.GetById(user.Id))
            .ReturnsAsync(user);
        // Act
        var result = await _userService.UpdateUser(updateUser, user.Id);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(updateUser.Name, result.Value.Name);
        _userRepository.Verify(repo => repo.GetById(It.IsAny<Guid>()), Times.Once);

    }
    [Fact]
    public async Task UpdateUser_WhenUserNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var updateUser = new UserUpdateDto { Name = "Updated Name" };

        _userRepository
            .Setup(repo => repo.GetById(user.Id))
            .ReturnsAsync((User)null!);
        // Act
        var result = await _userService.UpdateUser(updateUser, user.Id);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.UserNotFound, result.Error);
        _userRepository.Verify(repo => repo.GetById(It.IsAny<Guid>()), Times.Once);
    }
}