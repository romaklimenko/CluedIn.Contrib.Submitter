using CluedIn.Contrib.Submitter.Types;
using CluedIn.Core.Data;
using EasyNetQ;
using static CluedIn.Contrib.Submitter.Helpers.ParsingHelper;

namespace CluedIn.Contrib.Submitter;

/// <summary>
///     Responsible to keep and share mapping context like:
///     configuration, results, errors.
/// </summary>
public class Context
{
    public readonly List<EntityCode> EntityCodeTemplates;

    // Configuration
    public readonly string? NameTemplate;
    public readonly EntityType EntityType;
    public readonly EntityCode OriginEntityCodeTemplate;
    public readonly string VocabularyPrefix;
    public readonly List<EntityEdge> IncomingEntityEdgeTemplates;
    public readonly List<EntityEdge> OutgoingEntityEdgeTemplates;

    public readonly Guid OrganizationId;

    // Mapping state
    public readonly List<string> Errors = [];
    public int AcceptedCount;
    public int ReceivedCount;

    // Request
    public readonly string QueryString;

    // Submission properties
    public readonly string SubmissionId = Guid.NewGuid().ToString();
    public readonly long SubmissionTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // RabbitMQ
    public IBus? Bus { get; set; }

    private Context(
        string queryString,
        Guid organizationId,
        string? nameTemplateString,
        EntityType entityType,
        EntityCode originEntityCodeTemplate,
        string vocabularyPrefix,
        List<EntityCode> entityCodeTemplates,
        List<EntityEdge> incomingEntityEdgeTemplates,
        List<EntityEdge> outgoingEntityEdgeTemplates)
    {
        QueryString = queryString;

        OrganizationId = organizationId;

        NameTemplate = nameTemplateString;
        EntityType = entityType;

        OriginEntityCodeTemplate = originEntityCodeTemplate;

        VocabularyPrefix = vocabularyPrefix;

        EntityCodeTemplates = entityCodeTemplates;
        IncomingEntityEdgeTemplates = incomingEntityEdgeTemplates;
        OutgoingEntityEdgeTemplates = outgoingEntityEdgeTemplates;
    }

    public static bool TryCreate(
        string queryString,
        Guid organizationId,
        string? nameTemplateString,
        string? entityTypeString,
        string? originEntityCodeTemplateString,
        string? vocabularyPrefixString,
        string? entityCodeTemplatessString,
        string? incomingEntityEdgeTemplatesString,
        string? outgoingEntityEdgeTemplatesString,
        out Context context,
        out List<string> errors)
    {
        errors = [];

        _ = TryParseEntityType(entityTypeString, out var entityType, ref errors);
        _ = TryParseEntityCode(entityType, originEntityCodeTemplateString, out var originEntityCodeTemplate,
            ref errors);
        _ = TryParseVocabularyPrefix(vocabularyPrefixString, ref errors);
        _ = TryParseEntityCodes(entityType, entityCodeTemplatessString, out var entityCodeTemplates, ref errors);
        _ = TryParseEntityEdges(
            originEntityCodeTemplate,
            incomingEntityEdgeTemplatesString,
            EdgeDirection.Incoming,
            out var incomingEntityEdges,
            ref errors);
        _ = TryParseEntityEdges(
            originEntityCodeTemplate,
            outgoingEntityEdgeTemplatesString,
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
            organizationId,
            nameTemplateString,
            entityType!,
            originEntityCodeTemplate!,
            vocabularyPrefixString!,
            entityCodeTemplates,
            incomingEntityEdges,
            outgoingEntityEdges);

        return true;
    }
}
