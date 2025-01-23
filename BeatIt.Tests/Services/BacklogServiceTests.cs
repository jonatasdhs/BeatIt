using BeatIt.Errors;
using BeatIt.Models;
using BeatIt.Repositories;
using BeatIt.Services.BacklogService;
using BeatIt.Services.GameService;
using BeatIt.Tests.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace BeatIt.Tests.Services;

public class BacklogServiceTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<ILogger<BacklogService>> _loggerMock;
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly BacklogService backlogService;
    private readonly ITestOutputHelper _output;

    public BacklogServiceTests(ITestOutputHelper output)
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _loggerMock = new Mock<ILogger<BacklogService>>();
        _gameServiceMock = new Mock<IGameService>();
        backlogService = new BacklogService(_gameRepositoryMock.Object, _gameServiceMock.Object, _loggerMock.Object);
        _output = output;
    }

    [Fact]
    public async Task AddBacklogGame_ShouldReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var game = new Game()
        {
            Id = 1,
            IgdbGameId = 2,
            Name = "Test Game",
        };
        _gameServiceMock
            .Setup(service => service.GetOrCreateGame(game.IgdbGameId))
            .ReturnsAsync(Result.Success(game));
        _gameRepositoryMock
            .Setup(repo => repo.BacklogGameExists(game, user.Id))
            .ReturnsAsync(false);
        _gameRepositoryMock
            .Setup(repo => repo.AddGameToBacklog(It.IsAny<Backlog>()))
            .Returns(Task.CompletedTask);
        // Act
        var result = await backlogService.AddGameToBacklog(user, game.IgdbGameId);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(game.Name, result.Value.GameName);
        _gameServiceMock.Verify(service => service.GetOrCreateGame(It.IsAny<int>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.BacklogGameExists(It.IsAny<Game>(), It.IsAny<Guid>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.AddGameToBacklog(It.IsAny<Backlog>()), Times.Once);

    }

    [Fact]
    public async Task AddBacklogGame_WhenGameAlreadyInBacklog_ShouldReturnConflict()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var game = TestData.GetValidGame();

        _gameServiceMock
            .Setup(service => service.GetOrCreateGame(game.Id))
            .ReturnsAsync(Result.Success(game));
        _gameRepositoryMock
            .Setup(repo => repo.BacklogGameExists(game, user.Id))
            .ReturnsAsync(true);
        // Act
        var result = await backlogService.AddGameToBacklog(user, game.Id);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(BacklogErrors.Conflict, result.Error);
        _gameServiceMock.Verify(service => service.GetOrCreateGame(It.IsAny<int>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.BacklogGameExists(It.IsAny<Game>(), It.IsAny<Guid>()), Times.Once);

    }

    [Fact]
    public async Task AddBacklogGame_WhenGameNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var game = TestData.GetValidGame();

        _gameServiceMock
            .Setup(service => service.GetOrCreateGame(game.Id))
            .ReturnsAsync(Result.Failure<Game>(GameErrors.NotFound));
        // Act
        var result = await backlogService.AddGameToBacklog(user, game.Id);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(BacklogErrors.NotFound, result.Error);
        _gameServiceMock.Verify(service => service.GetOrCreateGame(It.IsAny<int>()), Times.Once);

    }

    [Fact]
    public async Task GetAllBacklogsFromUsers_ShouldReturnList()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var backlogs = TestData.ValidBacklogResponses();
        _gameRepositoryMock
            .Setup(repo => repo.GetAllBacklogWithGame(user.Id))
            .ReturnsAsync(backlogs);
        // Act
        var result = await backlogService.GetAllBacklogsFromUsers(user);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, backlogs);
        _gameRepositoryMock.Verify(repo => repo.GetAllBacklogWithGame(It.IsAny<Guid>()), Times.Once);

    }

    [Fact]
    public async Task RemoveGameFromBacklog_ShouldReturnSuccess()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var game = TestData.GetValidGame();
        var backlog = TestData.ValidBacklog();

        _gameRepositoryMock
            .Setup(repo => repo.GetById(game.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(repo => repo.BacklogGameExists(game, user.Id))
            .ReturnsAsync(true);
        _gameRepositoryMock
            .Setup(repo => repo.GetBacklogByIdAndUserId(game.Id, user.Id))
            .ReturnsAsync(backlog);
        _gameRepositoryMock
            .Setup(repo => repo.RemoveGameFromBacklog(backlog))
            .Returns(Task.CompletedTask);
        // Act
        var result = await backlogService.RemoveGameFromBacklog(user.Id, game.Id);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Game removed from backlog with success!", result.Value);
        _gameRepositoryMock.Verify(repo => repo.GetById(It.IsAny<int>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.BacklogGameExists(It.IsAny<Game>(), It.IsAny<Guid>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.GetBacklogByIdAndUserId(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.RemoveGameFromBacklog(It.IsAny<Backlog>()), Times.Once);

    }
    [Fact]
    public async Task RemoveGameFromBacklog_BacklogNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var game = TestData.GetValidGame();
        var backlog = TestData.ValidBacklog();

        _gameRepositoryMock
            .Setup(repo => repo.GetById(game.Id))
            .ReturnsAsync(game);
        _gameRepositoryMock
            .Setup(repo => repo.BacklogGameExists(game, user.Id))
            .ReturnsAsync(false);
        // Act
        var result = await backlogService.RemoveGameFromBacklog(user.Id, game.Id);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(BacklogErrors.NotFound, result.Error);
        _gameRepositoryMock.Verify(repo => repo.GetById(It.IsAny<int>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.BacklogGameExists(It.IsAny<Game>(), It.IsAny<Guid>()), Times.Once);

    }
    [Fact]
    public async Task RemoveGameFromBacklog_GameNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var game = TestData.GetValidGame();
        var backlog = TestData.ValidBacklog();

        _gameRepositoryMock
            .Setup(repo => repo.GetById(game.Id))
            .ReturnsAsync((Game)null!);
        // Act
        var result = await backlogService.RemoveGameFromBacklog(user.Id, game.Id);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GameErrors.NotFound, result.Error);
        _gameRepositoryMock.Verify(repo => repo.GetById(It.IsAny<int>()), Times.Once);
    }
}