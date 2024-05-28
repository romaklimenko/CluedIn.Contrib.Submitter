using System.Text.Json.Serialization;

namespace CluedIn.Contrib.Submitter.WebApi;

public record SubmitDataResponse
{
    [JsonPropertyName("submission")] public required dynamic Submission { get; set; }

    [JsonPropertyName("query_string")] public required string QueryString { get; set; }

    [JsonPropertyName("configuration")] public required Dictionary<string, string?> Configuration { get; set; }

    [JsonPropertyName("status")] public required dynamic Status { get; set; }

    [JsonPropertyName("errors")] public List<string> Errors { get; set; } = [];

    [JsonPropertyName("records")] public required Dictionary<string, int> Records { get; set; }
}
