using BeatIt.Models.DTOs;
using BeatIt.Models;

namespace BeatIt.Services.AuthService
{
    public interface IAuthService
    {
        Task<Result<TokenDto>> Login(UserLoginDto login);
        Task<Result<string>> SendResetEmail(string email);
        Task<Result<string>> ResetPassword(string newPassword, string token);
        Task<Result<TokenDto>> RefreshToken(string refreshToken);
        Task<Result> Logout(string refreshToken); 
    }
}