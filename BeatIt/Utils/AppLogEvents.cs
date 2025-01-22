using Microsoft.Extensions.Logging;

internal static class AppLogEvents
{
    internal static EventId Create = new((int)CRUDEvents.Create, "Created");
    internal static EventId Read = new((int)CRUDEvents.Read, "Read");
    internal static EventId Update = new((int)CRUDEvents.Update, "Updated");
    internal static EventId Delete = new((int)CRUDEvents.Delete, "Deleted");
    internal static EventId LoginFailed = new((int)AuthenticationEvents.UserLoginSuccess, "Login successful");
    internal static EventId LoginSuccessful = new((int)AuthenticationEvents.UserLoginFailed, "Login failed");
    internal static EventId InvalidValidation = new((int)ValidationEvents.InputValidationFailed, "Invalid request");
    internal const int Details = 3000;
    internal const int Error = 3001;
    internal static EventId ReadNotFound = 4000;
    internal static EventId UpdateNotFound = 4001;

}

enum CRUDEvents {
    Create = 1000,
    Read = 1001,
    Update = 1002,
    Delete = 1003,
}

enum AuthenticationEvents {
    UserLoginSuccess = 2000,
    UserLoginFailed = 2001,
    TokenRefreshed = 2002,
    AccessDenied = 2003
}

enum ValidationEvents
{
    InputValidationFailed = 3000,
    MissingRequiredField = 3001
}

enum SystemEvents
{
    DatabaseError = 5000,
    ServiceUnavailable = 5001
}