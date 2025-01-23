using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using BeatIt.Services.CacheService;
using BeatIt.Services.IgdbService;
using BeatIt.Utils;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Abstractions;

namespace BeatIt.Tests.Services;

public class IgdbServiceTests
{
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IOptions<IgdbConfig>> _configMock;
    private readonly Mock<ILogger<IgdbService>> _loggerMock;
    private readonly IgdbService _igdbService;
    private readonly ITestOutputHelper _outputHelper;
    public IgdbServiceTests(ITestOutputHelper outputHelper)
    {
        _cacheMock = new Mock<ICacheService>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configMock = new Mock<IOptions<IgdbConfig>>();
        _loggerMock = new Mock<ILogger<IgdbService>>();
        _outputHelper = outputHelper;

        _configMock.Setup(config => config.Value).Returns(new IgdbConfig
        {
            Client_Id = "testClientId",
            Secret_Key = "testSecretKey"
        });

        _igdbService = new IgdbService(_cacheMock.Object, _httpClientFactoryMock.Object, _configMock.Object, _loggerMock.Object);

    }

    [Fact]
    public async Task GetGameFromIgdbAsync_ReturnsGame_WhenGameExists()
    {
        // Arrange
        var messageHandler = new Mock<HttpMessageHandler>();
        var gameId = 123;
        var accessToken = "test_access_token";
        var igdbResponse = new IgdbResponse[] { new IgdbResponse { Id = gameId, Name = "Test Game" } };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(igdbResponse), Encoding.UTF8, "application/json")
        };

        _cacheMock.Setup(c => c.GetAsync("IGDB_AccessToken")).ReturnsAsync(accessToken);

        var mockedProtected = messageHandler.Protected();
        mockedProtected.Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
        var client = new HttpClient(messageHandler.Object);

        _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(client);

        // Act
        var result = await _igdbService.GetGameFromIgdbAsync(gameId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameId, result.Id);
        Assert.Equal("Test Game", result.Name);
    }

    [Fact]
    public async Task GetGameFromIgdbAsync_WhenGameNotExists_ReturnNull()
    {
        // Arrange
        var messageHandler = new Mock<HttpMessageHandler>();
        var gameId = 123;
        var accessToken = "test_access_token";
        var igdbResponse = new IgdbResponse[] { };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(igdbResponse), Encoding.UTF8, "application/json")
        };

        _cacheMock.Setup(c => c.GetAsync("IGDB_AccessToken")).ReturnsAsync(accessToken);

        var mockedProtected = messageHandler.Protected();
        mockedProtected.Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
        var client = new HttpClient(messageHandler.Object);

        _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(client);

        // Act
        var result = await _igdbService.GetGameFromIgdbAsync(gameId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetIgdbAccessTokenAsync_WhenTokenExists_ReturnCachedToken()
    {
        // Arrange
        var cachedToken = "cached_token";
        _cacheMock
            .Setup(cache => cache.GetAsync("IGDB_AccessToken"))
            .ReturnsAsync(cachedToken);

        // Act
        var result = await _igdbService.GetIgdbAccessTokenAsync();
        _outputHelper.WriteLine(result);
        // Assert
        Assert.Equal(cachedToken, result);
    }

    [Fact]
    public async Task GetIgdbAccessTokenAsync_WhenTokenDoesNotExists_RequestNewToken()
    {
        // Arrange
        var key = "Test_Key";
        _cacheMock.Setup(c => c.GetAsync(key))!.ReturnsAsync((string?)null);

        var tokenResponse = new CustomTokenResponse { AccessToken = "new_token", ExpiresIn = 3600 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
        };

        var clientHandlerMock = new Mock<HttpMessageHandler>();
        var mockedProtected = clientHandlerMock.Protected();
        mockedProtected.Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
        var client = new HttpClient(clientHandlerMock.Object);

        _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(client);
        // Act
        var result = await _igdbService.GetIgdbAccessTokenAsync();

        // Assert
        Assert.Equal("new_token", result);
    }

    [Fact]
    public async Task RequestIgdbTokenAsync_ReturnToken()
    {
        // Arrange

        var tokenResponse = new CustomTokenResponse { AccessToken = "new_token", ExpiresIn = 3600 };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
        };

        var clientHandlerMock = new Mock<HttpMessageHandler>();
        var mockedProtected = clientHandlerMock.Protected();
        mockedProtected.Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
        var client = new HttpClient(clientHandlerMock.Object);

        _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(client);
        // Act
        var result = await _igdbService.GetIgdbAccessTokenAsync();

        // Assert
        Assert.Equal("new_token", result);
    }


}