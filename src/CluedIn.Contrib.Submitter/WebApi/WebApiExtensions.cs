using System.Net;
using static Microsoft.AspNetCore.WebUtilities.ReasonPhrases;

namespace CluedIn.Contrib.Submitter.WebApi;

public static class WebApiExtensions
{
    /// <summary>
    /// Converts Context to IResult with status code.
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="httpStatusCode">HttpStatusCode</param>
    /// <returns>Result.Json</returns>
    public static IResult ToResult(this Context context, HttpStatusCode? httpStatusCode = null)
    {
        httpStatusCode ??= context.HasErrors
            ? HttpStatusCode.InternalServerError
            : HttpStatusCode.OK;

        var submitDataResponse = new SubmitDataResponse
        {
            QueryString = context.QueryString,
            SubmissionId = context.SubmissionId,
            SubmissionTimestamp = context.SubmissionTimestamp,
            StatusCode = (int)httpStatusCode,
            StatusDescription = GetReasonPhrase((int)httpStatusCode),
            Errors = context.Errors,
            ReceivedCount = context.ReceivedCount,
            AcceptedCount = context.AcceptedCount,
            RejectedCount = context.RejectedCount
        };

        return Results.Json(submitDataResponse, statusCode: (int)httpStatusCode);
    }
}
