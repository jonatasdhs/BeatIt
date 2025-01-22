using System.Text;
using System.Text.Json.Serialization;
using BeatIt.DataContext;
using BeatIt.Exceptions;
using BeatIt.Filters;
using BeatIt.Repositories;
using BeatIt.Services.AuthService;
using BeatIt.Services.BacklogService;
using BeatIt.Services.CacheService;
using BeatIt.Services.CompletedService;
using BeatIt.Services.GameService;
using BeatIt.Services.IgdbService;
using BeatIt.Services.IGDBService;
using BeatIt.Services.PasswordService;
using BeatIt.Services.TokenService;
using BeatIt.Services.UserService;
using BeatIt.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using StackExchange.Redis;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    Environment.SetEnvironmentVariable("IGDB_CLIENT_ID", "4j8fo2mq7aqpqlsfxqt3la2x2h1tg3", EnvironmentVariableTarget.Process);
    Environment.SetEnvironmentVariable("IGDB_SECRET_KEY", "q6zonsio48jdbt6srmkb717ezxw6lu", EnvironmentVariableTarget.Process);
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();

    builder.Host.UseNLog();

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidateModelAttribute>();
    })
    .AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IGameRepository, GameRepository>();
    builder.Services.AddScoped<IIgdbService, IgdbService>();
    builder.Services.AddScoped<IGameService, GameService>();
    builder.Services.AddScoped<IBacklogService, BacklogService>();
    builder.Services.AddScoped<ICompletedService, CompletedService>();
    builder.Services.AddScoped<ICacheService, CacheService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IPasswordService, PasswordService>();
    builder.Services.AddScoped<UserAuthenticationFilter>();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
    });

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        var redisConfigs = builder.Configuration.GetSection("RedisConfigurations").Get<RedisConfiguration>() ?? throw new MissingFieldException("Missing redis configs");
        var redisConfiguration = new ConfigurationOptions
        {
            /* Password = redisConfigs.Password, */
            SyncTimeout = redisConfigs.SyncTimeout
        };
        redisConfiguration.EndPoints.Add(redisConfigs.Endpoint);
        options.ConfigurationOptions = redisConfiguration;
        options.InstanceName = "BeatIt_";
    });

    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    ctx.Request.Cookies.TryGetValue("accessToken", out var accessToken);
                    if (!string.IsNullOrEmpty(accessToken))
                        ctx.Token = accessToken;
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!context.Database.CanConnect())
        {
            context.Database.Migrate();
        }
    }

    app.Logger.LogInformation("Starting the app");
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseExceptionHandler();
    app.MapControllers();
    app.Run();

}
catch (Exception ex)
{
    logger.Error(ex, "Application failed to start");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}