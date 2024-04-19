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
        rateLimiterOptions => rateLimiterOptions
            .AddConcurrencyLimiter(
                concurrencyLimiterPermitLimit,
                concurrencyLimiterOptions =>
                {
                    concurrencyLimiterOptions.PermitLimit =
                        GetIntegerEnvironmentVariable(concurrencyLimiterPermitLimit, 1);
                }));

builder.WebHost.ConfigureKestrel(kestrelServerOptions =>
{
    kestrelServerOptions.Limits.MaxRequestBodySize =
        GetIntegerEnvironmentVariable("KESTREL_MAX_REQUEST_BODY_SIZE", 1024 * 1024 * 256); // 128MB
});

builder.Services.AddRequestDecompression();
builder.Services.AddAuthentication(authenticationOptions =>
    {
        authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwtBearerOptions =>
    {
        var authUrl = Environment.GetEnvironmentVariable("AUTH_API");
        jwtBearerOptions.Authority = authUrl;
        jwtBearerOptions.Audience = "PublicApi";
        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = authUrl, ValidateIssuerSigningKey = true
        };
        jwtBearerOptions.SaveToken = true;
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

app.MapGet("/", () => Results.Text("CluedIn.Contrib.Submitter is running!"));
app.MapGet("/index.html", () => Results.LocalRedirect("/", true, true));

app.MapPost("/data", Endpoints.SubmitData)
    .RequireAuthorization(apiTokenPolicy)
    .RequireRateLimiting(concurrencyLimiterPermitLimit);

app.Run();
