using BeatIt.Errors;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Repositories;
using BeatIt.Services.CompletedService;
using BeatIt.Services.GameService;
using BeatIt.Tests.Models;
using Moq;
using Xunit;

namespace BeatIt.Tests.Services;

public class CompletedServiceTests
{
    private readonly CompletedService _completedService;
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly Mock<IGameRepository> _gameRepositoryMock;

    public CompletedServiceTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _gameRepositoryMock = new Mock<IGameRepository>();
        _completedService = new CompletedService(_gameRepositoryMock.Object, _gameServiceMock.Object);
    }

    [Fact]
    public async Task AddGameToCompletedList_ShouldReturnSuccess()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var game = TestData.GetValidGame();
        var completedGame = TestData.GetCompletedValid;
        var completedGameDto = TestData.GetCompletedGameDto;

        _gameServiceMock
            .Setup(service => service.GetOrCreateGame(game.IgdbGameId))
            .ReturnsAsync(Result.Success(game));
        _gameRepositoryMock
            .Setup(repo => repo.CompletedGameExists(game, user.Id))
            .ReturnsAsync(false);
        _gameRepositoryMock
            .Setup(repo => repo.AddGameToCompletedGames(completedGame))
            .Returns(Task.CompletedTask);
        // Act
        var result = await _completedService.AddGameToCompletedList(user, game.IgdbGameId, completedGameDto);
        // Assert       
        Assert.True(result.IsSuccess);
        Assert.Equal("Game added with success", result.Value);
        _gameServiceMock.Verify(service => service.GetOrCreateGame(It.IsAny<int>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.CompletedGameExists(It.IsAny<Game>(), It.IsAny<Guid>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.AddGameToCompletedGames(It.IsAny<CompletedGames>()), Times.Once);

    }

    [Fact]
    public async Task AddGameToCompletedList_WhenGameNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var game = TestData.GetValidGame();
        var completedGame = TestData.GetCompletedValid;

        _gameServiceMock
            .Setup(service => service.GetOrCreateGame(game.IgdbGameId))
            .ReturnsAsync(Result.Failure<Game>(GameErrors.NotFound));
        // Act
        var result = await _completedService.AddGameToCompletedList(user, game.IgdbGameId, It.IsAny<CompletedGameDto>());
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(GameErrors.NotFound, result.Error);
        _gameServiceMock.Verify(service => service.GetOrCreateGame(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task AddGameToCompletedList_WhenCompletedGameExists_ShouldReturnNotFound()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var game = TestData.GetValidGame();
        var completedGame = TestData.GetCompletedValid;

        _gameServiceMock
            .Setup(service => service.GetOrCreateGame(game.IgdbGameId))
            .ReturnsAsync(Result.Success(game));
        _gameRepositoryMock
            .Setup(repo => repo.CompletedGameExists(game, user.Id))
            .ReturnsAsync(true);
        // Act
        var result = await _completedService.AddGameToCompletedList(user, game.IgdbGameId, It.IsAny<CompletedGameDto>());
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(GameErrors.Conflict, result.Error);
        _gameServiceMock.Verify(service => service.GetOrCreateGame(It.IsAny<int>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.CompletedGameExists(It.IsAny<Game>(), It.IsAny<Guid>()), Times.Once);

    }

    [Fact]
    public async Task GetAllCompletedGames_ShouldReturnList()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var completedGameDto = TestData.GetCompletedGames(user.Id);
        _gameRepositoryMock
            .Setup(repo => repo.GetAllCompletedGames(user.Id))
            .ReturnsAsync(completedGameDto);
        // Act        
        var result = await _completedService.GetAllCompletedGames(user);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(completedGameDto.Count, result.Value.Count);
        _gameRepositoryMock.Verify(repo => repo.GetAllCompletedGames(It.IsAny<Guid>()), Times.Once);
    }
}