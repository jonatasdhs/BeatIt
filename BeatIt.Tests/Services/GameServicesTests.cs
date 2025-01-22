using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Repositories;
using BeatIt.Services.GameService;
using BeatIt.Services.IGDBService;
using BeatIt.Services.IgdbService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using BeatIt.Errors;
using Xunit.Abstractions;

namespace BeatIt.Tests.Services;

public class GameServicesTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<ILogger<GameService>> _loggerMock;
    private readonly Mock<IIgdbService> _igdbServicesMock;
    private readonly GameService gameServices;
    private readonly ITestOutputHelper _output;

    public GameServicesTests(ITestOutputHelper output)
    {
        _output = output;
        _gameRepositoryMock = new Mock<IGameRepository>();
        _loggerMock = new Mock<ILogger<GameService>>();
        _igdbServicesMock = new Mock<IIgdbService>();
        gameServices = new GameService(_igdbServicesMock.Object, _loggerMock.Object, _gameRepositoryMock.Object);
    }

    [Fact]
    public async Task AddGameToCompletedList_ShouldReturnSuccess()
    {
        var user = new User { Id = Guid.NewGuid() };
        var gameId = 1;
        var completedGameDto = new CompletedGameDto
        {
            Rating = 8,
            Notes = "Great game!",
            TimeToComplete = new TimeSpan(30),
            FinishedDate = DateTime.Now,
            Platform = "PC",
            Difficulty = 3,
            StartDate = DateTime.Now.AddDays(-10)
        };

        var igdbResponse = new IgdbResponse
        {
            Id = gameId,
            Name = "Test Game"
        };

        _gameRepositoryMock
            .Setup(repo => repo.CompletedGameExists(gameId, user.Id))
            .ReturnsAsync(false);
        _gameRepositoryMock
            .Setup(repo => repo.GetById(gameId))
            .ReturnsAsync((Game?)null);
        _igdbServicesMock
            .Setup(service => service.GetGameFromIgdbAsync(gameId))
            .ReturnsAsync(igdbResponse);
        _gameRepositoryMock
            .Setup(repo => repo.AddGame(It.IsAny<Game>()))
            .ReturnsAsync(new Game { IgdbGameId = igdbResponse.Id, Name = igdbResponse.Name });
        _gameRepositoryMock
            .Setup(repo => repo.AddGameToCompletedGames(It.IsAny<CompletedGames>()))
            .Returns(Task.CompletedTask);

        var result = await gameServices.AddGameToCompletedList(user, gameId, completedGameDto);

        Assert.True(result.IsSuccess);
        Assert.Equal("Game added with success", result.Value);

        _gameRepositoryMock.Verify(repo => repo.CompletedGameExists(gameId, user.Id), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.GetById(gameId), Times.Once);
        _igdbServicesMock.Verify(service => service.GetGameFromIgdbAsync(gameId), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.AddGame(It.IsAny<Game>()), Times.Once);
        _gameRepositoryMock.Verify(repo => repo.AddGameToCompletedGames(It.IsAny<CompletedGames>()), Times.Once);
    }

    [Fact]
    public async Task AddGameToCompletedList_ShouldReturnConflict_WhenCompletedGameExists()
    {
        var user = new User { Id = Guid.NewGuid() };
        var gameId = 1;
        CompletedGames completedGames = new() { User = user, GameId = gameId };
        CompletedGameDto completedGameDto = new()
        {
            Rating = 8,
            Notes = "Great game!",
            TimeToComplete = new TimeSpan(30),
            FinishedDate = DateTime.Now,
            Platform = "PC",
            Difficulty = 3,
            StartDate = DateTime.Now.AddDays(-10)
        };
        _gameRepositoryMock
            .Setup(repo => repo.CompletedGameExists(gameId, user.Id))
            .ReturnsAsync(true);

        var result = await gameServices.AddGameToCompletedList(user, gameId, completedGameDto);
        _output.WriteLine($"Result: {result.Error}");
        Assert.True(result.IsFailure);
        Assert.Equal(GameErrors.Conflict, result.Error);
    }

    [Fact]
    public async Task AddGameToCompletedList_ShoudReturnNotFound_WhenGameNotFoundInRepoAndIgdb()
    {
        var user = new User { Id = Guid.NewGuid() };
        var gameId = 1;
        var completedGameDto = new CompletedGameDto
        {
            Rating = 8,
            Platform = "Pc"
        };

        _gameRepositoryMock
            .Setup(repo => repo.CompletedGameExists(gameId, user.Id))
            .ReturnsAsync(false);

        _gameRepositoryMock
            .Setup(repo => repo.GetById(gameId))
            .ReturnsAsync((Game?)null);

        _igdbServicesMock
            .Setup(service => service.GetGameFromIgdbAsync(gameId))
            .ReturnsAsync((IgdbResponse?)null);

        var result = await gameServices.AddGameToCompletedList(user, gameId, completedGameDto);

        Assert.True(result.IsFailure);
        Assert.Equal(GameErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task AddGameToBacklog_ShouldReturnSuccess()
    {
        var user = new User { Id = Guid.NewGuid() };
        var gameId = 1;

        var igdbResponse = new IgdbResponse
        {
            Id = gameId,
            Name = "Game Name",
        };

        _gameRepositoryMock
            .Setup(repo => repo.BacklogGameExists(gameId, user.Id))
            .ReturnsAsync(false);
        _gameRepositoryMock
            .Setup(repo => repo.GetById(gameId))
            .ReturnsAsync((Game?)null);
        _igdbServicesMock
            .Setup(service => service.GetGameFromIgdbAsync(gameId))
            .ReturnsAsync(igdbResponse);
        _gameRepositoryMock
            .Setup(repo => repo.AddGame(It.IsAny<Game>()))
            .ReturnsAsync(new Game { IgdbGameId = igdbResponse.Id, Name = igdbResponse.Name });
        _gameRepositoryMock
            .Setup(repo => repo.AddGameToBacklog(It.IsAny<Backlog>()))
            .Returns(Task.CompletedTask);

        var result = await gameServices.AddGameToBacklog(user, gameId);

        Assert.True(result.IsSuccess);
        /* Assert.Equal("Game added to backlog with success", result.Value); */
    }

    [Fact]
    public async Task AddGameToBacklog_ShouldReturnConflit_WhenBacklogAlreadyExists()
    {
        var user = new User { Id = Guid.NewGuid() };
        var gameId = 1;
        var backlog = new Backlog() { GameId = gameId, UserId = user.Id };

        _gameRepositoryMock
            .Setup(repo => repo.BacklogGameExists(gameId, user.Id))
            .ReturnsAsync(true);

        var result = await gameServices.AddGameToBacklog(user, gameId);
        _output.WriteLine($"Result: {result.Error}");
        Assert.True(result.IsFailure);
        Assert.Equal(GameErrors.Conflict, result.Error);
    }

    [Fact]
    public async Task AddGameToBacklog_ShouldReturnNotFound_WhenGameNotFoundInRepoAndIgdb()
    {
        var user = new User { Id = Guid.NewGuid() };
        var gameId = 1;

        _gameRepositoryMock
            .Setup(repo => repo.CompletedGameExists(gameId, user.Id))
            .ReturnsAsync(false);

        _gameRepositoryMock
            .Setup(repo => repo.GetById(gameId))
            .ReturnsAsync((Game?)null);

        _igdbServicesMock
            .Setup(service => service.GetGameFromIgdbAsync(gameId))
            .ReturnsAsync((IgdbResponse?)null);

        var result = await gameServices.AddGameToBacklog(user, gameId);
        _output.WriteLine($"Results: {result.IsSuccess}");
        Assert.True(result.IsFailure);
        Assert.Equal(GameErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task RemoveGameFromBacklog_ShouldReturnSuccess()
    {
        var user = new User { Id = Guid.NewGuid() };
        var gameId = 1;

        _gameRepositoryMock
            .Setup(repo => repo.BacklogGameExists(gameId, user.Id))
            .ReturnsAsync(true);

        var result = await gameServices.RemoveGameFromBacklog(user.Id, gameId);

        Assert.True(result.IsSuccess);
        Assert.Equal("Game removed from backlog with success!", result.Value);
    }

    [Fact]
    public async Task RemoveGameFromBacklog_ShouldReturnNotFound_WhenBacklogNotExists()
    {
        var user = new User { Id = Guid.NewGuid() };
        var gameId = 1;

        _gameRepositoryMock
            .Setup(repo => repo.BacklogGameExists(gameId, user.Id))
            .ReturnsAsync(false);

        var result = await gameServices.RemoveGameFromBacklog(user.Id, gameId);

        Assert.True(result.IsFailure);
        Assert.Equal(GameErrors.NotFound, result.Error);
    }
}