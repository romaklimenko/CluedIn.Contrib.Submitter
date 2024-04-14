namespace CluedIn.Contrib.Submitter;

/// <summary>
///     Responsible to keep and share mapping context like:
///     configuration, results, errors.
/// </summary>
public class Context
{

    // Configuration
    public readonly string? EntityTypeConfiguration;
    public readonly string? OriginEntityCodeConfiguration;
    public readonly string? EntityCodesConfiguration;
    public readonly string? VocabularyPrefixConfiguration;
    public readonly string? IncomingEntityEdgesConfiguration;
    public readonly string? OutgoingEntityEdgesConfiguration;

    // Request
    public readonly string QueryString;

    // Mapping state
    public readonly List<string> Errors = [];
    public int ReceivedCount;
    public int AcceptedCount;
    public int RejectedCount;

    // Submission properties
    public readonly string SubmissionId = Guid.NewGuid().ToString();
    public readonly long SubmissionTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public Context(
        string queryString,
        string? entityTypeConfiguration,
        string? originEntityCodeConfiguration,
        string? vocabularyPrefixConfiguration,
        string? entityCodesConfiguration,
        string? incomingEntityEdgesConfiguration,
        string? outgoingEntityEdgesConfiguration)
    {
        QueryString = queryString;
        EntityTypeConfiguration = entityTypeConfiguration;
        if (string.IsNullOrWhiteSpace(EntityTypeConfiguration))
        {
            Errors.Add("Missing the mandatory 'entity_type' query string parameter.");
        }

        OriginEntityCodeConfiguration = originEntityCodeConfiguration;
        if (string.IsNullOrWhiteSpace(OriginEntityCodeConfiguration))
        {
            Errors.Add("Missing the mandatory 'origin_entity_code' query string parameter.");
        }

        VocabularyPrefixConfiguration = vocabularyPrefixConfiguration;
        if (string.IsNullOrWhiteSpace(VocabularyPrefixConfiguration))
        {
            Errors.Add("Missing the mandatory 'vocabulary_prefix' query string parameter.");
        }

        EntityCodesConfiguration = entityCodesConfiguration;
        IncomingEntityEdgesConfiguration = incomingEntityEdgesConfiguration;
        OutgoingEntityEdgesConfiguration = outgoingEntityEdgesConfiguration;
    }

    public bool HasErrors => Errors.Count > 0;
}
