using BeatIt.Models;

namespace BeatIt.Errors;
public static class BacklogErrors
{
    public static readonly Error NotFound = Error.NotFound("Game.NotFound", "Username or Password is invalid");
    public static readonly Error InvalidToken = Error.NotFound("Game.InvalidToken", "Token is invalid");
    public static readonly Error EmailNotFound = Error.NotFound("Game.NotFound", "Email not exists");
    public static readonly Error BadRequest = Error.Validation("Game.BadRequest", "Bad request");
    public static readonly Error Conflict = Error.Conflict("Game.Conflict", "Backlog game already exists");
}