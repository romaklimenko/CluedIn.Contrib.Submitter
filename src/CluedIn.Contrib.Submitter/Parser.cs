using CluedIn.Core.Data;

namespace CluedIn.Contrib.Submitter;

public static class Parser
{
    public static bool TryParseEntityType(string? entityTypeString, out EntityType? entityType, ref List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(entityTypeString))
        {
            errors.Add("Entity Type must not be null or empty string.");
            entityType = null;
            return false;
        }

        try
        {
            entityType = entityTypeString;
            return true;
        }
        catch (Exception e)
        {
            errors.Add(
                $"Error when parsing Entity Type. Provided value: '{entityTypeString}'. Exception: {e.Message}.");
            entityType = null;
            return false;
        }
    }

    public static bool TryParseEntityCode(
        EntityType? entityType,
        string? entityCodeString,
        out EntityCode? entityCode,
        ref List<string> errors)
    {
        if (entityType != null)
        {
            return TryParseEntityCode($"{entityType}#{entityCodeString}", out entityCode, ref errors);
        }

        errors.Add("Can't parse Entity Code if Entity Type is null or empty string.");
        entityCode = null;
        return false;
    }

    public static bool TryParseEntityCode(string? entityCodeString, out EntityCode? entityCode, ref List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(entityCodeString))
        {
            errors.Add("Entity Code must not be null or empty string.");
            entityCode = null;
            return false;
        }

        try
        {
            entityCode = EntityCode.FromKey(entityCodeString);
            return true;
        }
        catch (Exception e)
        {
            errors.Add(
                $"Error when parsing Entity Code. Provided value: '{entityCodeString}'. Exception: {e.Message}.");
            entityCode = null;
            return false;
        }
    }

    public static bool TryParseVocabularyPrefix(string? vocabularyPrefixString, ref List<string> errors)
    {
        if (!string.IsNullOrWhiteSpace(vocabularyPrefixString))
        {
            return true;
        }

        errors.Add("Vocabulary Prefix must not be null or empty string.");
        return false;
    }

    public static bool TryParseEntityCodes(
        EntityType? entityType,
        string? entityCodesString,
        out List<EntityCode> entityCodes,
        ref List<string> errors)
    {
        entityCodes = [];

        if (string.IsNullOrWhiteSpace(entityCodesString))
        {
            return true;
        }

        foreach (var entityCodeString in entityCodesString.Split(',',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (TryParseEntityCode(entityType, entityCodeString, out var entityCode, ref errors))
            {
                entityCodes.Add(entityCode!);
            }
            else
            {
                entityCodes.Clear();
                return false;
            }
        }

        return true;
    }

    public static bool TryParseEntityEdge(
        EntityCode? knownEntityCode,
        string? entityEdgeString,
        EdgeDirection edgeDirection,
        out EntityEdge? entityEdge,
        ref List<string> errors)
    {
        // /WorksAt|/Organization#Salesforce:id

        entityEdge = null;

        if (knownEntityCode == null)
        {
            errors.Add("Can't parse an Entity Edge without Origin Entity Code.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(entityEdgeString))
        {
            errors.Add("Can't parse an Entity Edge from an empty string or null.");
            return false;
        }

        try
        {
            var parts = entityEdgeString.Split('|',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                errors.Add($"Could not parse '{entityEdgeString}' into an Entity Edge.");
                return false;
            }

            if (!TryParseEntityCode(parts[1], out var parsedEntityCode, ref errors))
            {
                errors.Add($"Could not parse '{entityEdgeString}' into an Entity Edge.");
                return false;
            }

            entityEdge = edgeDirection switch
            {
                EdgeDirection.Incoming => new EntityEdge(new EntityReference(parsedEntityCode),
                    new EntityReference(knownEntityCode), parts[0]),
                EdgeDirection.Outgoing => new EntityEdge(new EntityReference(knownEntityCode),
                    new EntityReference(parsedEntityCode), parts[0]),
                _ => throw new ArgumentOutOfRangeException(nameof(edgeDirection), edgeDirection, null)
            };
            return true;

            return false;
        }
        catch (Exception e)
        {
            errors.Add(
                $"Error when parsing Entity Edge. Provided value: '{entityEdgeString}'. Exception: {e.Message}.");
            return false;
        }
    }

    public static bool TryParseEntityEdges(
        EntityCode? knownEntityCode,
        string? entityEdgesString,
        EdgeDirection edgeDirection,
        out List<EntityEdge> entityEdges,
        ref List<string> errors)
    {
        entityEdges = [];

        if (string.IsNullOrWhiteSpace(entityEdgesString))
        {
            return true;
        }

        foreach (var entityEdgeString in entityEdgesString
                     .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (TryParseEntityEdge(
                    knownEntityCode,
                    entityEdgeString,
                    edgeDirection,
                    out var parsedEntityEdge,
                    ref errors))
            {
                entityEdges.Add(parsedEntityEdge!);
            }
            else
            {
                entityEdges.Clear();
                return false;
            }
        }

        return true;
    }
}
