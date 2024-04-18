using System.Net;
using System.Text.Json;
using CluedIn.Contrib.Submitter.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace CluedIn.Contrib.Submitter.WebApi;

public static class Endpoints
{
    public static async Task<IResult> SubmitData(
        HttpContext httpContext,
        [FromQuery(Name = "name")] string? nameConfigString = null,
        [FromQuery(Name = "entity_type")] string? entityTypeConfigString = null,
        [FromQuery(Name = "origin_code")] string? originCodeConfigString = null,
        [FromQuery(Name = "vocab_prefix")] string? vocabPrefixConfigString = null,
        [FromQuery(Name = "codes")] string? codesConfigString = null,
        [FromQuery(Name = "incoming_edges")] string? incomingEdgesConfigString = null,
        [FromQuery(Name = "outgoing_edges")] string? outgoingEdgesConfig = null)
    {
        // Step 1: Context
        var errors = new List<string>();

        if (!Guid.TryParse(
                httpContext.User.Claims
                    .FirstOrDefault(x => x.Type == "OrganizationId")?.Value,
                out var organizationId))
        {
            errors.Add("Missing the mandatory 'OrganizationId' Authorization header parameter.");
        }

        if (string.IsNullOrWhiteSpace(entityTypeConfigString))
        {
            errors.Add("Missing the mandatory 'entity_type' query string parameter.");
        }

        if (string.IsNullOrWhiteSpace(codesConfigString))
        {
            errors.Add("Missing the mandatory 'code' query string parameter.");
        }

        if (string.IsNullOrWhiteSpace(vocabPrefixConfigString))
        {
            errors.Add("Missing the mandatory 'vocab_prefix' query string parameter.");
        }

        if (errors.Count > 0)
        {
            return Results.BadRequest(
                new
                {
                    query_string = httpContext.Request.QueryString.Value,
                    status_code = 400,
                    status_description = "Bad Request",
                    errors
                });
        }

        if (!Context.TryCreate(
                httpContext.Request.QueryString.ToString(),
                organizationId,
                nameConfigString,
                entityTypeConfigString,
                originCodeConfigString,
                vocabPrefixConfigString,
                codesConfigString,
                incomingEdgesConfigString,
                outgoingEdgesConfig,
                out var context,
                out errors))
        {
            return Results.BadRequest(
                new
                {
                    query_string = httpContext.Request.QueryString,
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
        context.ReceivedCount = jsonDocument.RootElement.GetArrayLength();
        foreach (var jsonElement in jsonDocument.RootElement.EnumerateArray())
        {
            try
            {
                if (jsonElement.Flatten().ToClue(context) == null)
                {
                    return context.ToResult(HttpStatusCode.BadRequest);
                }

                context.AcceptedCount++;
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

        async Task<JsonDocument?> TryParseJsonDocument()
        {
            try
            {
                var doc = await JsonDocument.ParseAsync(httpContext.Request.Body);
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
