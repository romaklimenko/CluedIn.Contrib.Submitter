using CluedIn.Contrib.Submitter.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRequestDecompression();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var authUrl = Environment.GetEnvironmentVariable("AUTH_API") ?? "http://cluedin-server:9001/";
        options.Authority = authUrl;
        options.Audience = "PublicApi";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = authUrl, ValidateIssuerSigningKey = true
        };
        options.SaveToken = true;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("api_token", policy =>
    {
        policy.RequireAuthenticatedUser()
            .RequireRole("API")
            .RequireClaim("scope", ["PublicApi", "ServerApiForUI"]);
    });

var app = builder.Build();

app.UseRequestDecompression();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");
app.MapGet("/index.html", () => Results.LocalRedirect("/", true, true));

app.MapPost("/data", Endpoints.SubmitData).RequireAuthorization("api_token");


app.Run();
