using CluedIn.Contrib.Submitter.Mapping;

namespace CluedIn.Contrib.Submitter;

/// <summary>
///     Responsible to keep and share mapping context like:
///     configuration, results, errors.
/// </summary>
public class Context
{
    public readonly List<CodeConfiguration> CodesConfiguration;

    // Configuration
    public readonly string EntityTypeConfiguration;
    public readonly CodeConfiguration OriginCodeConfiguration;
    public readonly string VocabularyPrefixConfiguration;
    public readonly List<EdgeConfiguration> IncomingEdgesConfiguration;
    public readonly List<EdgeConfiguration> OutgoingEdgesConfiguration;

    // TODO: get from JWT
    public readonly Guid OrganizationId = Guid.NewGuid();

    // Mapping state
    public readonly List<string> Errors = [];
    public int AcceptedCount;
    public int ReceivedCount;
    public int RejectedCount;

    // Request
    public readonly string QueryString;

    // Submission properties
    public readonly string SubmissionId = Guid.NewGuid().ToString();
    public readonly long SubmissionTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private Context(
        string queryString,
        string entityTypeConfiguration,
        CodeConfiguration originCodeConfiguration,
        string vocabularyPrefixConfiguration,
        List<CodeConfiguration> codesConfiguration,
        List<EdgeConfiguration> incomingEdgesConfiguration,
        List<EdgeConfiguration> outgoingEdgesConfiguration)
    {
        QueryString = queryString;
        EntityTypeConfiguration = entityTypeConfiguration;

        OriginCodeConfiguration = originCodeConfiguration;

        VocabularyPrefixConfiguration = vocabularyPrefixConfiguration;

        CodesConfiguration = codesConfiguration;
        IncomingEdgesConfiguration = incomingEdgesConfiguration;
        OutgoingEdgesConfiguration = outgoingEdgesConfiguration;
    }

    public static bool TryCreate(
        string queryString,
        string? entityTypeConfigString,
        string? originCodeConfigString,
        string? vocabPrefixConfigString,
        string? codesConfigString,
        string? incomingEdgesConfigString,
        string? outgoingEdgesConfigString,
        out Context context,
        out List<string> errors)
    {
        errors = [];

        // Entity Type
        if (string.IsNullOrWhiteSpace(entityTypeConfigString))
        {
            errors.Add("Missing the mandatory 'entity_type' query string parameter.");
        }

        // Origin Entity Code
        var originCodeConfig = ParseCodeConfiguration(originCodeConfigString, errors);

        // Vocabulary Prefix
        if (string.IsNullOrWhiteSpace(vocabPrefixConfigString))
        {
            errors.Add("Missing the mandatory 'vocab_prefix' query string parameter.");
        }

        // Entity Codes
        var codesConfig = ParseCodesConfiguration(codesConfigString, errors);

        // Incoming Edges
        var incomingEdgesParsed = ParseEdgesConfiguration(incomingEdgesConfigString, errors);

        // Outgoing Edges
        var outgoingEdgesParsed = ParseEdgesConfiguration(outgoingEdgesConfigString, errors);

        if (errors.Count > 0)
        {
            context = null!;
            return false;
        }

        context = new Context(
            queryString,
            entityTypeConfigString!,
            originCodeConfig!.Value,
            vocabPrefixConfigString!,
            codesConfig,
            incomingEdgesParsed,
            outgoingEdgesParsed);

        return true;

        CodeConfiguration? ParseCodeConfiguration(string? codeConfigStringLocal, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(codeConfigStringLocal))
            {
                errors.Add("Missing the mandatory 'origin_code' query string parameter.");
                return null;
            }

            var parts = codeConfigStringLocal.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 &&
                !string.IsNullOrWhiteSpace(parts[0]) &&
                !string.IsNullOrWhiteSpace(parts[1]))
            {
                return new CodeConfiguration(parts[0], parts[1]);
            }

            errors.Add("Could not parse the Entity Code in the query string parameter's value " +
                       "The value must be in the format 'Origin:Id', " +
                       $"but found ''{codeConfigStringLocal}''");
            return null;
        }

        List<CodeConfiguration> ParseCodesConfiguration(string? codesConfigStringLocal, ICollection<string> errors)
        {
            var codeConfigurations = new List<CodeConfiguration>();
            if (string.IsNullOrWhiteSpace(codesConfigStringLocal))
            {
                return codeConfigurations;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var codeConfigString in codesConfigStringLocal.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var codeConfiguration = ParseCodeConfiguration(codeConfigString, errors);
                if (codeConfiguration != null)
                {
                    codeConfigurations.Add(codeConfiguration.Value);
                }
            }

            return codeConfigurations;
        }

        List<EdgeConfiguration> ParseEdgesConfiguration(string? edgesConfigStringLocal, List<string> errors)
        {
            var edgesConfigurationParsed = new List<EdgeConfiguration>();
            if (string.IsNullOrWhiteSpace(edgesConfigStringLocal))
            {
                return edgesConfigurationParsed;
            }

            foreach (var edgeConfiguration in edgesConfigStringLocal
                         .Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                // /At|/Organization#Salesforce:organization_id
                var partsByPipe = edgeConfiguration.Split('|', StringSplitOptions.RemoveEmptyEntries);
                var entityEdgeType = partsByPipe[0];
                var partsByHash = partsByPipe.ElementAtOrDefault(1)?.Split('#', StringSplitOptions.RemoveEmptyEntries);
                var entityType = partsByHash?.ElementAtOrDefault(0);
                var partsByColon = partsByHash?
                    .ElementAtOrDefault(1)?
                    .Split(':', StringSplitOptions.RemoveEmptyEntries);
                var origin = partsByColon?.ElementAtOrDefault(0);
                var idPropertyName = partsByColon?.ElementAtOrDefault(1);

                if (!string.IsNullOrWhiteSpace(entityEdgeType) &&
                    !string.IsNullOrWhiteSpace(entityType) &&
                    !string.IsNullOrWhiteSpace(origin) &&
                    !string.IsNullOrWhiteSpace(idPropertyName))
                {
                    edgesConfigurationParsed.Add(
                        new EdgeConfiguration(
                            entityEdgeType, entityType, origin, idPropertyName));
                }
                else
                {
                    errors.Add("Could not parse entity edge configuration. " +
                               "The value must be in the format '/EdgeType|/EntityType#Origin:id', " +
                               $"but found: '{edgeConfiguration}'");
                }
            }

            return edgesConfigurationParsed;
        }
    }
}
