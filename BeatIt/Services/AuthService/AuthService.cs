using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Errors;
using BeatIt.Repositories;
using BeatIt.Services.TokenService;
using BeatIt.Services.PasswordService;
using BeatIt.Services.CacheService;

namespace BeatIt.Services.AuthService
{
    public class AuthService(IUserRepository userRepository, ICacheService cache, IPasswordService hasher, ITokenService tokenService, ILogger<AuthService> logger) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly ICacheService _cache = cache;
        private readonly IPasswordService _hasher = hasher;
        private readonly ILogger<AuthService> _logger = logger;

        public async Task<Result<TokenDto>> Login(UserLoginDto login)
        {
            var user = await _userRepository.GetByEmail(login.Email);
            if (user == null)
            {
                _logger.LogInformation(AppLogEvents.LoginFailed, "Login failed - invalid email/password");
                return Result.Failure<TokenDto>(AuthErrors.NotFound);
            }

            if (!_hasher.VerifyPassword(login.Password, user.Password, user.Salt))
            {
                _logger.LogInformation("Login failed - invalid email/password");
                return Result.Failure<TokenDto>(AuthErrors.NotFound);
            }

            var token = _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var cacheKey = $"userId: {user.Id}";
            await _cache.StoreOnCache(cacheKey, refreshToken, TimeSpan.FromDays(30));

            TokenDto dto = new(token, $"{user.Id}:{refreshToken}");
            _logger.LogInformation("Login successful");
            return Result.Success(dto);
        }


        public async Task<Result<TokenDto>> RefreshToken(string refreshToken)
        {
            var refreshTokenValues = refreshToken.Split(":");
            
            if (refreshTokenValues.Length != 2)
            {
                return Result.Failure<TokenDto>(AuthErrors.InvalidToken);
            }
            var user = await _userRepository.GetById(Guid.Parse(refreshTokenValues[0]));
            if (user == null)
            {
                return Result.Failure<TokenDto>(AuthErrors.InvalidToken);
            }
            string cacheKey = $"userId: {user.Id}";
            var refreshTokenCache = await _cache.GetFromCache(cacheKey);
            if (refreshTokenCache is null || refreshTokenCache != refreshTokenValues[1])
            {
                return Result.Failure<TokenDto>(AuthErrors.InvalidToken);
            }


            var accessToken = _tokenService.GenerateJwtToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            cacheKey = $"userId: {user.Id}";
            await _cache.StoreOnCache(cacheKey, newRefreshToken, TimeSpan.FromDays(30));

            var dto = new TokenDto(accessToken, $"{user.Id}:{newRefreshToken}");

            return Result.Success(dto);
        }
        public async Task<Result<string>> SendResetEmail(string email)
        {
            var user = await _userRepository.GetByEmail(email);
            if (user is null)
            {
                return Result.Failure<string>(AuthErrors.EmailNotFound);
            }

            var guid = _tokenService.GenerateRefreshToken();
            string cacheKey = $"reset-token: ${user.Id}";
            await _cache.StoreOnCache(cacheKey, guid, TimeSpan.FromMinutes(15));

            return Result.Success("Email was sent if success!");
        }
        public async Task<Result<string>> ResetPassword(string newPassword, string token)
        {
            string cacheKey = $"reset-token: ${token}";
            var userId = await _cache.GetFromCache(cacheKey);
            if (userId is null)
            {
                return Result.Failure<string>(AuthErrors.NotFound);
            }

            var user = await _userRepository.GetById(Guid.Parse(userId));

            if (user is null)
            {
                return Result.Failure<string>(AuthErrors.NotFound);
            }

            var salt = _hasher.GenerateSalt();
            var hashedPassword = _hasher.HashPassword(newPassword, salt);

            user.Password = hashedPassword;
            user.Salt = Convert.ToBase64String(salt);

            await _userRepository.Update(user);

            return Result.Success("Email was sent if success!");
        }
    }
}