using BeatIt.Models;

namespace BeatIt.Errors;
public static class BacklogErrors
{
    public static readonly Error NotFound = Error.NotFound("Backlog.NotFound", "Username or Password is invalid");
    public static readonly Error InvalidToken = Error.NotFound("Backlog.InvalidToken", "Token is invalid");
    public static readonly Error EmailNotFound = Error.NotFound("Backlog.NotFound", "Email not exists");
    public static readonly Error BadRequest = Error.Validation("Backlog.BadRequest", "Bad request");
    public static readonly Error Conflict = Error.Conflict("Backlog.Conflict", "Backlog game already exists");
}