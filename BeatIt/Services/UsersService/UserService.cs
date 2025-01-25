using BeatIt.Models;
using BeatIt.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using BeatIt.Services.PasswordService;
using BeatIt.Errors;
using BeatIt.Repositories;

namespace BeatIt.Services.UserService
{
    public class UserService(IUserRepository userRepository, IPasswordService hasher) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordService _hasher = hasher;
        public async Task<Result<UserDto>> CreateUser([FromBody] UserCreateDto newUser)
        {
            var user = await _userRepository.GetByEmail(newUser.Email);
            if (user != null)
            {
                return Result.Failure<UserDto>(UserErrors.AlreadyExists);
            }
            var salt = _hasher.GenerateSalt();
            var hashedPassword = _hasher.HashPassword(newUser.Password, salt);
            var appUser = new User
            {
                Name = newUser.Name,
                Email = newUser.Email,
                Password = hashedPassword,
                Salt = Convert.ToBase64String(salt),
                Created_at = DateTime.Now.ToLocalTime(),
                Updated_at = DateTime.Now.ToLocalTime()
            };

            await _userRepository.Add(appUser);

            UserDto userDto = new()
            {
                Id = appUser.Id,
                Name = appUser.Name,
                Email = appUser.Email,
                Created_at = appUser.Created_at,
                Updated_at = appUser.Updated_at
            };

            return Result.Success(userDto);
        }

        public async Task<Result<UserDto>> GetUserById(Guid userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
            {
                return Result.Failure<UserDto>(UserErrors.UserNotFound);
            }
            UserDto userDto = new()
            {
                Email = user.Email,
                Name = user.Name,
                Created_at = user.Created_at,
                Updated_at = user.Updated_at,
                Id = user.Id,
                IsActive = user.IsActive
            };
            return Result.Success(userDto);
        }
        public async Task<Result<List<UserDto>>> GetUsers()
        {
            var users = await _userRepository.GetAllUsers();
            var userDtos = users.Select(user => new UserDto
            {
                Email = user.Email,
                Name = user.Name,
                Created_at = user.Created_at,
                Updated_at = user.Updated_at,
                Id = user.Id,
                IsActive = user.IsActive
            }).ToList();
            return Result.Success(userDtos);
        }

        public async Task<Result<string>> SoftDelete(Guid userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
            {
                return Result.Failure<string>(UserErrors.UserNotFound);
            }
            user.IsActive = false;
            await _userRepository.Update(user);
            return Result.Success("User deleted Successfully");
        }

        public async Task<Result<UserDto>> UpdateUser(UserUpdateDto updateUser, Guid userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
            {
                return Result.Failure<UserDto>(UserErrors.UserNotFound);
            }
            user.Name = updateUser.Name ?? user.Name;
            user.Email = updateUser.Email ?? user.Email;
            user.Updated_at = DateTime.UtcNow.ToLocalTime();
            await _userRepository.Update(user);
            var userDto = new UserDto
            {
                Email = user.Email,
                Name = user.Name,
                Created_at = user.Created_at,
                Updated_at = user.Updated_at,
                Id = user.Id,
                IsActive = user.IsActive
            };
            return Result.Success(userDto);
        }
    }
}