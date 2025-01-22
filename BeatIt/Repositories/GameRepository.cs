using BeatIt.DataContext;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BeatIt.Repositories;

public class GameRepository(ApplicationDbContext context) : IGameRepository
{
    private readonly ApplicationDbContext _context = context;
    public async Task<Game?> GetById(int id)
    {
        return await _context.Game.FirstOrDefaultAsync(game => game.IgdbGameId == id) ?? null;
    }

    public async Task<bool> CompletedGameExists(Game game, Guid userId)
    {
        var gameExists = await _context.CompletedGames.AnyAsync(cg => cg.GameId == game.Id && cg.UserId == userId);
        return gameExists;
    }

    public async Task<bool> BacklogGameExists(Game game, Guid userId)
    {
        return await _context.Backlog.AnyAsync(bg => bg.GameId == game.Id && bg.UserId == userId);
    }

    public async Task<Backlog?> GetBacklogByIdAndUserId(int gameId, Guid userId)
    {
        var backlog = await _context.Backlog.FirstOrDefaultAsync(bg => bg.GameId == gameId && bg.UserId == userId);
        return backlog ?? null;
    }
    public async Task<Game> AddGame(Game game)
    {
        _context.Game.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }
    public async Task AddGameToBacklog(Backlog backlog)
    {
        _context.Backlog.Add(backlog);
        await _context.SaveChangesAsync();
    }
    public async Task AddGameToCompletedGames(CompletedGames completedGames)
    {
        _context.CompletedGames.Add(completedGames);
        await _context.SaveChangesAsync();
    }
    public async Task RemoveGameFromBacklog(Backlog backlog)
    {
        _context.Backlog.Remove(backlog);
        await _context.SaveChangesAsync();
    }
    public async Task<List<Backlog>> GetAllBacklog(Guid userId)
    {
        return await _context.Backlog.Where(bg => bg.UserId == userId)
        .ToListAsync();
    }
    public async Task<List<CompletedGames>> GetAllCompletedGames(Guid userId)
    {
        var completedGames = await _context.CompletedGames
        .Where(cg => cg.UserId == userId)
        .Include(cg => cg.Game)
        .ToListAsync();
        return completedGames;
    }

    public async Task<List<BacklogResponse>> GetAllBacklogWithGame(Guid userId)
    {
        var backlogGames = from backlog in _context.Backlog
                           join game in _context.Game
                           on backlog.GameId equals game.Id
                           where backlog.UserId == userId
                           select new BacklogResponse
                           {
                               GameName = game.Name,
                               Created_at = backlog.Created_at
                           };
        return await backlogGames.ToListAsync();
    }

    public async Task<List<Game>> GetAllGames()
    {
        return await _context.Game.ToListAsync();
    }
}