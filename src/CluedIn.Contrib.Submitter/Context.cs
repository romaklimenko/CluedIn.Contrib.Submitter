using CluedIn.Core.Data;
using static CluedIn.Contrib.Submitter.Parser;

namespace CluedIn.Contrib.Submitter;

/// <summary>
///     Responsible to keep and share mapping context like:
///     configuration, results, errors.
/// </summary>
public class Context
{
    public readonly List<EntityCode> EntityCodes;

    // Configuration
    public readonly EntityType EntityType;
    public readonly EntityCode OriginEntityCode;
    public readonly string VocabularyPrefix;
    public readonly List<EntityEdge> IncomingEntityEdges;
    public readonly List<EntityEdge> OutgoingEntityEdges;

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
        EntityType entityType,
        EntityCode originEntityCode,
        string vocabularyPrefix,
        List<EntityCode> entityCodes,
        List<EntityEdge> incomingEntityEdges,
        List<EntityEdge> outgoingEntityEdges)
    {
        QueryString = queryString;
        EntityType = entityType;

        OriginEntityCode = originEntityCode;

        VocabularyPrefix = vocabularyPrefix;

        EntityCodes = entityCodes;
        IncomingEntityEdges = incomingEntityEdges;
        OutgoingEntityEdges = outgoingEntityEdges;
    }

    public static bool TryCreate(
        string queryString,
        string? entityTypeString,
        string? originEntityCodeString,
        string? vocabularyPrefixString,
        string? entityCodesString,
        string? incomingEntityEdgesString,
        string? outgoingEntityEdgesString,
        out Context context,
        out List<string> errors)
    {
        errors = [];

        _ = TryParseEntityType(entityTypeString, out var entityType, ref errors);
        _ = TryParseEntityCode(entityType, originEntityCodeString, out var originEntityCode, ref errors);
        _ = TryParseVocabularyPrefix(vocabularyPrefixString, ref errors);
        _ = TryParseEntityCodes(entityType, entityCodesString, out var entityCodes, ref errors);
        _ = TryParseEntityEdges(
            originEntityCode,
            incomingEntityEdgesString,
            EdgeDirection.Incoming,
            out var incomingEntityEdges,
            ref errors);
        _ = TryParseEntityEdges(
            originEntityCode,
            outgoingEntityEdgesString,
            EdgeDirection.Outgoing,
            out var outgoingEntityEdges,
            ref errors);

        if (errors.Count > 0)
        {
            context = null!;
            return false;
        }

        context = new Context(
            queryString,
            entityType!,
            originEntityCode!,
            vocabularyPrefixString!,
            entityCodes,
            incomingEntityEdges,
            outgoingEntityEdges);

        return true;
    }
}
