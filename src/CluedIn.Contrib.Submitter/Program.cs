using CluedIn.Contrib.Submitter.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using static CluedIn.Contrib.Submitter.Helpers.EnvironmentHelper;

const string apiTokenPolicy = "API_TOKEN_POLICY";
const string concurrencyLimiterPermitLimit = "CONCURRENCY_LIMITER_PERMIT_LIMIT";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRateLimiter(
        _ => _
            .AddConcurrencyLimiter(
                concurrencyLimiterPermitLimit,
                options =>
                {
                    options.PermitLimit =
                        GetIntegerEnvironmentVariable(concurrencyLimiterPermitLimit, 1);
                }));

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        GetIntegerEnvironmentVariable("KESTREL_MAX_REQUEST_BODY_SIZE", 1024 * 1024 * 256); // 128MB
});

builder.Services.AddRequestDecompression();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var authUrl = GetStringEnvironmentVariable("AUTH_API", "http://cluedin-server:9001/");
        options.Authority = authUrl;
        options.Audience = "PublicApi";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = authUrl, ValidateIssuerSigningKey = true
        };
        options.SaveToken = true;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(apiTokenPolicy, policy =>
    {
        policy.RequireAuthenticatedUser()
            .RequireRole("API")
            .RequireClaim("scope", ["PublicApi", "ServerApiForUI"]);
    });

var app = builder.Build();

app.UseRateLimiter();
app.UseRequestDecompression();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");
app.MapGet("/index.html", () => Results.LocalRedirect("/", true, true));

app.MapPost("/data", Endpoints.SubmitData)
    .RequireAuthorization(apiTokenPolicy)
    .RequireRateLimiting(concurrencyLimiterPermitLimit);

app.Run();
