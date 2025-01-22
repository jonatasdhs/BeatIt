namespace BeatIt.Middlewares;

public class Middlewares
{
    private readonly RequestDelegate _next;
    public Middlewares(RequestDelegate next)
    {
        _next = next;
    }
    static void HandleInvalidRequest(IApplicationBuilder app)
    {
        app.Run(async context => 
        {
            await context.Response.WriteAsync("Map Test 1");
        });
    }

}