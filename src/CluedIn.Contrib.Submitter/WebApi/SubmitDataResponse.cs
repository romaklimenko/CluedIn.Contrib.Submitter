using System.Text.Json.Serialization;

namespace CluedIn.Contrib.Submitter.WebApi;

public record SubmitDataResponse
{
    [JsonPropertyName("submission_id")]
    public required string SubmissionId { get; set; }

    [JsonPropertyName("submission_timestamp")]
    public required long SubmissionTimestamp { get; set; }

    [JsonPropertyName("query_string")]
    public required string QueryString { get; set; }

    [JsonPropertyName("status_code")]
    public required int? StatusCode { get; set; }

    [JsonPropertyName("status_description")]
    public required string? StatusDescription { get; set; }

    [JsonPropertyName("errors")] public List<string> Errors { get; set; } = [];

    [JsonPropertyName("received_count")] public required int ReceivedCount { get; set; } = 0;

    [JsonPropertyName("accepted_count")] public required int AcceptedCount { get; set; } = 0;

    [JsonPropertyName("rejected_count")] public required int RejectedCount { get; set; } = 0;
}
