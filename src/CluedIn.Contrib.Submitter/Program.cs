using CluedIn.Contrib.Submitter;
using CluedIn.Contrib.Submitter.WebApi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("/index.html", () => Results.LocalRedirect("/", true, true));

app.MapPost("/data", Endpoints.SubmitData);

app.Run();
