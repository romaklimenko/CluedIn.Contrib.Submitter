using Castle.Windsor;
using CluedIn.Core;
using CluedIn.Core.Agent;
using CluedIn.Core.Data;
using CluedIn.Core.Messages.Processing;
using CluedIn.Core.Serialization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var applicationContext = new ApplicationContext(new WindsorContainer());

app.MapGet("/", () => "Hello World!");

app.MapGet("/submit/dry-run", (HttpContext http) =>
{
    var clue = new Clue(
        new EntityCode("/Person", "Submitter", Guid.NewGuid()),
        Guid.NewGuid());
    var compressedClue = CompressedClue.Compress(clue, applicationContext);
    var command = new ProcessClueCommand(new JobRunId(Guid.NewGuid(), Guid.NewGuid()), compressedClue);
    return Results.Json(new { clues = new List<string> { clue.Serialize() }, status = "OK" });
});

app.Run();
