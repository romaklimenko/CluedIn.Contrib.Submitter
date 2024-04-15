using System.Net;
using System.Text.Json;
using Castle.Windsor;
using CluedIn.Contrib.Submitter.Mapping;
using CluedIn.Core;
using Microsoft.AspNetCore.Mvc;

namespace CluedIn.Contrib.Submitter.WebApi;

public static class Endpoints
{
    private static readonly ApplicationContext s_applicationContext = new(new WindsorContainer());

    public static async Task<IResult> SubmitData(
        HttpRequest httpRequest,
        [FromQuery(Name = "entity_type")] string? entityTypeConfigString = null,
        [FromQuery(Name = "origin_code")] string? originCodeConfigString = null,
        [FromQuery(Name = "vocab_prefix")] string? vocabPrefixConfigString = null,
        [FromQuery(Name = "codes")] string? codesConfigString = null,
        [FromQuery(Name = "in_edges")] string? incomingEdgesConfigString = null,
        [FromQuery(Name = "out_edges")] string? outgoingEdgesConfig = null,
        [FromHeader(Name = "Authorization")] string? authorization = null)
    {
        // TODO: Step 0: Authorization

        // Step 1: Context
        if (!Context.TryCreate(
                httpRequest.QueryString.ToString(),
                entityTypeConfigString,
                originCodeConfigString,
                vocabPrefixConfigString,
                codesConfigString,
                incomingEdgesConfigString,
                outgoingEdgesConfig,
                out var context,
                out var errors))
        {
            return Results.BadRequest(
                new
                {
                    query_string = httpRequest.QueryString,
                    status_code = 400,
                    status_description = "Bad Request",
                    errors
                });
        }

        // Step2: Deserialize and validate payload
        using var jsonDocument = await TryParseJsonDocument();
        if (jsonDocument is null)
        {
            return context.ToResult(HttpStatusCode.BadRequest);
        }


        // Step 3: For each record in the payload, create and publish a clue
        foreach (var jsonElement in jsonDocument.RootElement.EnumerateArray())
        {
            try
            {
                jsonElement.Flatten().ToClue(context);
            }
            catch (Exception e)
            {
                context.Errors.Add(
                    $"Error when processing a record.\n" +
                    $"Record:{jsonElement.ToString()}\n" +
                    $"Exception:{e}\n{e.Message}");
                return context.ToResult(HttpStatusCode.BadRequest);
            }
        }

        // Step 4: Response
        return context.ToResult(HttpStatusCode.OK);

        // var clue = new Clue(
        //     new EntityCode("/Person", "Submitter", Guid.NewGuid()),
        //     Guid.NewGuid());
        // var compressedClue = CompressedClue.Compress(clue, applicationContext);
        // var command = new ProcessClueCommand(new JobRunId(Guid.NewGuid(), Guid.NewGuid()), compressedClue);
        // return Results.Json(new { clues = new List<string> { clue.Serialize() }, status = "OK" });

        async Task<JsonDocument?> TryParseJsonDocument()
        {
            try
            {
                var doc = await JsonDocument.ParseAsync(httpRequest.Body);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return doc;
                }

                context.Errors.Add("The payload must be a JSON array.");
                return null;
            }
            catch (Exception e)
            {
                context.Errors.Add($"Couldn't parse JSON payload: \n{e}\n{e.Message}");
                return null;
            }
        }
    }
}
