using BeatIt.Models;

namespace BeatIt.Errors;
public static class AuthErrors
{
    public static readonly Error NotFound = Error.NotFound("Auth.NotFound", "Username or Password is invalid");
    public static readonly Error InvalidToken = Error.NotFound("Auth.InvalidToken", "Token is invalid");
    public static readonly Error EmailNotFound = Error.NotFound("Auth.NotFound", "Email not exists");
    public static readonly Error BadRequest = Error.Validation("Auth.BadRequest", "Bad request");
}