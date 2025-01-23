using System.Text;
using BeatIt.Services.PasswordService;
using Xunit;

namespace BeatIt.Tests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;
    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_WhenValidPassword_ReturnsHashedPassword()
    {
        // Arrange
        var password = "password";
        var salt = Encoding.UTF8.GetBytes("salt");

        // Act
        var hashedPassword = _passwordService.HashPassword(password, salt);

        // Assert
        Assert.NotEqual(password, hashedPassword);
    }

    [Fact]
    public void GenerateSalt_Returns16Bytes()
    {
        // Act
        var salt = _passwordService.GenerateSalt();

        // Assert
        Assert.NotNull(salt);
        Assert.Equal(16, salt.Length);
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashesForDifferentSalts()
    {
        // Arrange
        var password = "TestPassword";
        var salt1 = _passwordService.GenerateSalt();
        var salt2 = _passwordService.GenerateSalt();

        // Act
        var hash1 = _passwordService.HashPassword(password, salt1);
        var hash2 = _passwordService.HashPassword(password, salt2);

        // Assert
        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_WhenValidPasswordAndSalt_ReturnTrue()
    {
        // Arrange
        var password = "ValidPassword";
        var salt = _passwordService.GenerateSalt();
        var hash = _passwordService.HashPassword(password, salt);
        var saltBase64 = Convert.ToBase64String(salt);

        // Act
        var isValid = _passwordService.VerifyPassword(password, hash, saltBase64);

        // Assert
        Assert.True(isValid);
    }
    [Fact]
    public void VerifyPassword_WhenInValidPasswordAndSalt_ReturnFalse()
    {
        // Arrange
        var password = "ValidPassword";
        var invalidPassword = "InValidPassword";
        var salt = _passwordService.GenerateSalt();
        var hash = _passwordService.HashPassword(password, salt);
        var saltBase64 = Convert.ToBase64String(salt);

        // Act
        var isValid = _passwordService.VerifyPassword(invalidPassword, hash, saltBase64);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForInvalidSalt()
    {
        // Arrange
        var password = "ValidPassword";
        var salt = _passwordService.GenerateSalt();
        var invalidSalt = _passwordService.GenerateSalt();
        var hash = _passwordService.HashPassword(password, salt);
        var invalidSaltBase64 = Convert.ToBase64String(invalidSalt);

        // Act
        var isValid = _passwordService.VerifyPassword(password, hash, invalidSaltBase64);

        // Assert
        Assert.False(isValid);
    }
}