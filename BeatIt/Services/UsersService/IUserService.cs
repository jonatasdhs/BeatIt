using BeatIt.Models.DTOs;
using BeatIt.Models;

namespace BeatIt.Services.UserService
{
    public interface IUserService
    {
        Task<Result<UserDto>> CreateUser(UserCreateDto newUser);
        Task<Result<UserDto>> UpdateUser(UserUpdateDto updateUser, Guid userId);
        Task<Result<string>> SoftDelete(Guid Id);
        Task<Result<List<UserDto>>> GetUsers();
        Task<Result<UserDto>> GetUserById(Guid userId);
    }
}