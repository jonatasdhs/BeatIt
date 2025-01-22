using BeatIt.Repositories;
using BeatIt.Services.TokenService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BeatIt.Filters;

public class UserAuthenticationFilter(IUserRepository userRepository, ITokenService tokenService): IAsyncActionFilter
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITokenService _tokenService = tokenService;
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var accessToken = context.HttpContext.Request.Cookies["AccessToken"];
        if (string.IsNullOrEmpty(accessToken)) {
            context.Result = new NotFoundObjectResult("Object not found!");
        }
        var userId = _tokenService.GetUserIdFromToken(accessToken!);
        if(userId == Guid.Empty) {
            context.Result = new NotFoundObjectResult("Object not found!");
        }
        var user = await _userRepository.GetById(userId);
        if (user == null) {
            context.Result = new NotFoundObjectResult("Object not found!");
        }
        context.HttpContext.Items["AuthenticateUser"] = user;
        await next();
    }
}