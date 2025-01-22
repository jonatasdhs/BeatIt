using BeatIt.Errors;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Repositories;
using BeatIt.Services.PasswordService;
using BeatIt.Services.UserService;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace BeatIt.Tests.Services;

public class UserServicesTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IPasswordService> _hasher;
    private readonly UserService _userServices;
    private readonly ITestOutputHelper _output;

    public UserServicesTests(ITestOutputHelper output)
    {
        _output = output;
        _userRepository = new Mock<IUserRepository>();
        _hasher = new Mock<IPasswordService>();
        _userServices = new UserService(_userRepository.Object, _hasher.Object);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnSuccess()
    {
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

        var result = await _userServices.CreateUser(newUser);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(newUser.Name, result.Value.Name);
        Assert.Equal(newUser.Email, result.Value.Email);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        var newUser = new UserCreateDto
        {
            Email = "test@mail.com",
            Password = "password",
            Name = "Teste",
        };

        _userRepository
            .Setup(repo => repo.GetByEmail(newUser.Email))
            .ReturnsAsync(new User());
        var result = await _userServices.CreateUser(newUser);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.AlreadyExists, result.Error);
    }
}