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
        HttpContext httpContext,
        [FromQuery(Name = "entity_type")] string? entityTypeConfiguration = null,
        [FromQuery(Name = "origin_entity_code")]
        string? originEntityCodeConfiguration = null,
        [FromQuery(Name = "vocabulary_prefix")]
        string? vocabularyPrefixConfiguration = null,
        [FromQuery(Name = "entity_codes")] string? entityCodesConfiguration = null,
        [FromQuery(Name = "incoming_edges")] string? incomingEntityEdgesConfiguration = null,
        [FromQuery(Name = "outgoing_edges")] string? outgoingEntityEdgesConfiguration = null,
        [FromHeader(Name = "Authorization")] string? authorization = null)
    {
        // TODO: Step 0: Authorization

        // Step 1: Context
        var context = new Context(
            httpContext.Request.QueryString.ToString(),
            entityTypeConfiguration,
            originEntityCodeConfiguration,
            vocabularyPrefixConfiguration,
            entityCodesConfiguration,
            incomingEntityEdgesConfiguration,
            outgoingEntityEdgesConfiguration);

        if (context.HasErrors)
        {
            return context.ToResult(HttpStatusCode.BadRequest);
        }

        // Step2: Deserialize and validate payload
        using var jsonDocument = await TryParseJsonDocument();
        if (jsonDocument != null && jsonDocument.RootElement.ValueKind != JsonValueKind.Array)
        {
            context.Errors.Add("The payload must be a JSON array.");
        }

        if (context.HasErrors || jsonDocument is null)
        {
            return context.ToResult(HttpStatusCode.BadRequest);
        }


        // TODO: Step 3: For each record in the payload, create and publish a clue
        foreach (var jsonElement in jsonDocument.RootElement.EnumerateArray())
        {
            jsonElement.Flatten().ToClue(context);
        }

        // TODO: Step 4: Response
        return context.ToResult();

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
                return await JsonDocument.ParseAsync(httpContext.Request.Body);
            }
            catch (Exception e)
            {
                context.Errors.Add($"Couldn't parse JSON payload: \n{e}\n{e.Message}");
                return null;
            }
        }
    }
}
