using BeatIt.Models;

namespace BeatIt.Repositories;

public interface IUserRepository {
    Task<User?> GetById(Guid guid);
    Task<User?> GetByEmail(string email);
    Task<List<User>> GetAllUsers();
    Task Add(User user);
    Task Update(User user);
}