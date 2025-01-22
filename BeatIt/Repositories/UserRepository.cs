using BeatIt.DataContext;
using BeatIt.Models;
using Microsoft.EntityFrameworkCore;

namespace BeatIt.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task Add(User user)
    {
        _context.User.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await _context.User.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<User?> GetById(Guid guid)
    {
        return await _context.User.FirstOrDefaultAsync(user => user.Id == guid);
    }

    public async Task Update(User user)
    {
        _context.User.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task<List<User>> GetAllUsers() 
    {
        return await _context.User.ToListAsync();
    }
}