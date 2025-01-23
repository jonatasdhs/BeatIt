using BeatIt.Models;
using BeatIt.Repositories;
using BeatIt.Services.GameService;
using BeatIt.Services.IgdbService;
using BeatIt.Services.IGDBService;
using BeatIt.Tests.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace BeatIt.Tests.Services;

public class GameServicesTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<ILogger<GameService>> _loggerMock;
    private readonly Mock<IIgdbService> _igdbServiceMock;
    private readonly GameService _gameService;
    private readonly ITestOutputHelper _output;

    public GameServicesTests(ITestOutputHelper output)
    {
        _output = output;
        _gameRepositoryMock = new Mock<IGameRepository>();
        _loggerMock = new Mock<ILogger<GameService>>();
        _igdbServiceMock = new Mock<IIgdbService>();
        _gameService = new GameService(_igdbServiceMock.Object, _loggerMock.Object, _gameRepositoryMock.Object);
    }

    [Fact]
    public async Task GetOrCreateGame_WhenFetchingFromGameRepository_ShouldReturnSuccess()
    {
        // Arrange
        var existingGame = TestData.GetValidGame();

        _gameRepositoryMock
            .Setup(repo => repo.GetById(existingGame.IgdbGameId))
            .ReturnsAsync(existingGame);
        // Act
        var result = await _gameService.GetOrCreateGame(existingGame.IgdbGameId);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(existingGame, result.Value);
        _gameRepositoryMock.Verify(repo => repo.GetById(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateGame_WhenFetchingFromIGDB_ShouldReturnSuccess()
    {
        // Arrange
        var gameId = 123;
        var igdbGame = new IgdbResponse { Id = gameId, Name = "New Game" };
        _gameRepositoryMock.Setup(repo => repo.GetById(gameId)).ReturnsAsync((Game)null!);
        _igdbServiceMock.Setup(service => service.GetGameFromIgdbAsync(gameId)).ReturnsAsync(igdbGame);
        // Act
        var result = await _gameService.GetOrCreateGame(gameId);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(igdbGame.Id, result.Value.IgdbGameId);
        Assert.Equal(igdbGame.Name, result.Value.Name);
        _gameRepositoryMock.Verify(repo => repo.AddGame(It.IsAny<Game>()), Times.Once);
        _igdbServiceMock.Verify(service => service.GetGameFromIgdbAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetAllGames_ShouldReturnGameList()
    {
        // Arrange
        var games = TestData.GetValidGames();
        _gameRepositoryMock.Setup(repo => repo.GetAllGames()).ReturnsAsync(games);
        // Act
        var result = await _gameService.GetAllGames();
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(games, result.Value);
        _gameRepositoryMock.Verify(repo => repo.GetAllGames(), Times.Once);
    }
}