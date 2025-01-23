using BeatIt.Services.CacheService;
using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BeatIt.Tests.Services;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<CacheService>> _loggerMock;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CacheService>>();
        _cacheService = new CacheService(_cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsCachedValue()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
        _cacheMock.Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(valueBytes);
        // Act
        var result = await _cacheService.GetAsync(key);
        // Assert
        Assert.Equal(value, result);
        _cacheMock.Verify(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenKeyNotFound_ReturnNull()
    {
        // Arrange
        var key = "testKey";
        _cacheMock.Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
        .ReturnsAsync((byte[]?)null);
        // Act
        var result = await _cacheService.GetAsync(key);
        // Assert
        Assert.Null(result);
        _cacheMock.Verify(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StoreOnCache_ShouldReturnSuccess()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
        var expiration = TimeSpan.FromMinutes(5);
        // Act
        await _cacheService.StoreOnCache(key, value, expiration);
        // Assert
        _cacheMock.Verify(cache => cache.SetAsync(key, valueBytes,
            It.Is<DistributedCacheEntryOptions>(options =>
                options.AbsoluteExpirationRelativeToNow == expiration),
            default), Times.Once);
    }
}