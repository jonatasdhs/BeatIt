using BeatIt.Models;
using BeatIt.Models.DTOs;

namespace BeatIt.Tests.Models;

public static class TestData
{
    public static User GetValidUser()
    {
        return new() { Id = Guid.NewGuid(), Email = "test@mail.com", Created_at = DateTime.Now, IsActive = true, Name = "Test" };
    }

    public static List<User> GetValidUsers()
    {
        return
        [
            new User { Id = Guid.NewGuid()},
            new User { Id = Guid.NewGuid()}
        ];
    }
    public static User GetInvalidUser()
    {
        return new() { Id = Guid.Empty };
    }
    public static List<Game> GetValidGames()
    {
        return new List<Game>
        {
            new Game { Id = 1, Name = "ValidGame1", IgdbGameId = 2 },
            new Game { Id = 2, Name = "ValidGame2", IgdbGameId = 3 },
            new Game { Id = 3, Name = "ValidGame3", IgdbGameId = 4 }
        };
    }

    public static Game GetValidGame()
    {
        return new Game
        {
            Id = 1,
            Name = "ValidGame1",
            IgdbGameId = 2
        };
    }

    public static List<BacklogResponse> ValidBacklogResponses()
    {
        return new List<BacklogResponse>
        {
            new BacklogResponse { GameName = "Game 1", Created_at = DateTime.Now },
            new BacklogResponse { GameName = "Game 2", Created_at = DateTime.Now }
        };
    }

    public static Backlog ValidBacklog()
    {
        return new Backlog
        {
            Id = 1,
            GameId = 1,
            Created_at = DateTime.Now

        };
    }
    public static CompletedGames GetCompletedValid => new CompletedGames
    {
        Id = 1,
        GameId = 101,
        UserId = Guid.NewGuid(),
        Difficulty = 5,
        Rating = 8,
        Notes = "Great game, challenging at the end.",
        FinishedDate = DateTime.UtcNow.AddDays(-1),
        TimeToComplete = TimeSpan.FromHours(15),
        Platform = "PC",
        StartDate = DateTime.UtcNow.AddDays(-16)
    };

    public static CompletedGames GetCompletedInvalid => new CompletedGames
    {
        Id = 1,
        GameId = 0,
        UserId = Guid.NewGuid(),
        Difficulty = 5,
        Rating = 8,
        Notes = "Invalid game ID.",
        FinishedDate = DateTime.UtcNow.AddDays(-1),
        TimeToComplete = TimeSpan.FromHours(15),
        Platform = "PC",
        StartDate = DateTime.UtcNow.AddDays(-16)
    };

    public static CompletedGameDto GetCompletedGameDto => new CompletedGameDto
    {
        Difficulty = 3,
        FinishedDate = DateTime.Now.AddDays(-1),
        Notes = "Ótimo jogo com uma narrativa envolvente.",
        Platform = "Pc",
        Rating = 7,
        StartDate = DateTime.Now.AddDays(-4),
        TimeToComplete = TimeSpan.FromHours(10),
    };

    public static List<CompletedGameDto> GetCompletedGameDtos()
    {
        return new List<CompletedGameDto>
        {
                        new CompletedGameDto
            {
                Difficulty = 3,
                Rating = 8,
                Notes = "Great game with challenging puzzles.",
                FinishedDate = new DateTime(2024, 1, 15),
                TimeToComplete = TimeSpan.FromHours(35),
                Platform = "PC",
                StartDate = new DateTime(2023, 12, 15)
            },
            new CompletedGameDto
            {
                Difficulty = 2,
                Rating = 7,
                Notes = "Enjoyed the story, but gameplay was repetitive.",
                FinishedDate = new DateTime(2024, 1, 10),
                TimeToComplete = TimeSpan.FromHours(25),
                Platform = "PlayStation 5",
                StartDate = new DateTime(2023, 12, 1)
            },
            new CompletedGameDto
            {
                Difficulty = 5,
                Rating = 9,
                Notes = "Amazing graphics and immersive world.",
                FinishedDate = new DateTime(2024, 1, 20),
                TimeToComplete = TimeSpan.FromHours(50),
                Platform = "Xbox Series X",
                StartDate = new DateTime(2023, 11, 5)
            }
        };
    }

    public static List<CompletedGames> GetCompletedGames(Guid userId)
    {
        var userId1 = Guid.NewGuid(); // Simulando um UserId para o primeiro usuário
        var userId2 = Guid.NewGuid(); // Simulando um UserId para o segundo usuário

        return new List<CompletedGames>
        {
            new CompletedGames
            {
                Id = 1,
                GameId = 101,
                UserId = userId,
                Difficulty = 3,
                Rating = 8,
                Notes = "Great game with challenging puzzles.",
                FinishedDate = new DateTime(2024, 1, 15),
                TimeToComplete = TimeSpan.FromHours(35),
                Platform = "PC",
                StartDate = new DateTime(2023, 12, 15)
            },
            new CompletedGames
            {
                Id = 2,
                GameId = 102,
                UserId = userId,
                Difficulty = 2,
                Rating = 7,
                Notes = "Enjoyed the story, but gameplay was repetitive.",
                FinishedDate = new DateTime(2024, 1, 10),
                TimeToComplete = TimeSpan.FromHours(25),
                Platform = "PlayStation 5",
                StartDate = new DateTime(2023, 12, 1)
            },
            new CompletedGames
            {
                Id = 3,
                GameId = 103, 
                UserId = userId,
                Difficulty = 5,
                Rating = 9,
                Notes = "Amazing graphics and immersive world.",
                FinishedDate = new DateTime(2024, 1, 20),
                TimeToComplete = TimeSpan.FromHours(50),
                Platform = "Xbox Series X",
                StartDate = new DateTime(2023, 11, 5)
            }
        };
    }
}