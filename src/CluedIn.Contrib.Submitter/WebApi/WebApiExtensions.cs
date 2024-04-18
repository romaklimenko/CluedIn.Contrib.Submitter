using System.Net;
using static Microsoft.AspNetCore.WebUtilities.ReasonPhrases;

namespace CluedIn.Contrib.Submitter.WebApi;

public static class WebApiExtensions
{
    /// <summary>
    ///     Converts Context to IResult with status code.
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="httpStatusCode">HttpStatusCode</param>
    /// <returns>Result.Json</returns>
    public static IResult ToResult(this Context context, HttpStatusCode httpStatusCode)
    {
        var submitDataResponse = new SubmitDataResponse
        {
            Submission = new { id = context.SubmissionId, timestamp = context.SubmissionTimestamp },
            QueryString = context.QueryString,
            Configuration = new Dictionary<string, string?>
            {
                { "name_template", context.NameTemplate },
                { "origin_entity_code_template", context.OriginEntityCodeTemplate.ToString() },
                { "entity_type", context.EntityType },
                { "vocabulary_prefix", context.VocabularyPrefix },
                {
                    "entity_code_templates",
                    string.Join(',', context.EntityCodeTemplates.Select(x => x.ToString()))
                },
                {
                    "incoming_edges_templates",
                    string.Join(',', context.IncomingEntityEdgeTemplates.Select(x => x.ToString()))
                },
                {
                    "outgoing_edges_templates",
                    string.Join(',', context.OutgoingEntityEdgeTemplates.Select(x => x.ToString()))
                }
            },
            Status = new { code = (int)httpStatusCode, description = GetReasonPhrase((int)httpStatusCode) },
            Errors = context.Errors,
            Records = new Dictionary<string, int>
            {
                { "received", context.ReceivedCount },
                { "accepted", context.AcceptedCount },
                { "rejected", context.ReceivedCount - context.AcceptedCount }
            }
        };

        return Results.Json(submitDataResponse, statusCode: (int)httpStatusCode);
    }
}
