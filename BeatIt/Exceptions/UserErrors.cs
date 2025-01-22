using BeatIt.Models;

namespace BeatIt.Errors;
public static class UserErrors
{
    public static readonly Error AlreadyExists = Error.Conflict("User.AlreadyExists", "Email already exists!");
    public static readonly Error InvalidToken = Error.NotFound("User.InvalidToken", "Token is invalid");
    public static readonly Error EmailNotFound = Error.NotFound("User.NotFound", "Email not exists");
    public static readonly Error BadRequest = Error.Validation("User.BadRequest", "Bad request");
    public static readonly Error UserNotFound = Error.Validation("User.NotFound", "User Id not found");
}