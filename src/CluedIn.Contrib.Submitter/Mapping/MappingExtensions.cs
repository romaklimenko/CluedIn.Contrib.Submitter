using System.Text.Json;
using CluedIn.Core.Data;

namespace CluedIn.Contrib.Submitter.Mapping;

public static class MappingExtensions
{
    public static Clue? ToClue(this Dictionary<string, string> data, Context context)
    {
        if (!data.TryGetValue(context.OriginEntityCodeTemplate.Value, out var originEntityCodeValue))
        {
            context.Errors.Add(
                $"Can't get Origin Entity Code value of property '{context.OriginEntityCodeTemplate.Value}' " +
                $"from record {data}.");
            return null;
        }

        var clue = new Clue(
            new EntityCode(
                context.OriginEntityCodeTemplate.Type,
                context.OriginEntityCodeTemplate.Origin,
                originEntityCodeValue),
            context.OrganizationId);

        var entityData = clue.Data.EntityData;

        // Name
        if (!string.IsNullOrWhiteSpace(context.NameTemplate) &&
            data.TryGetValue(context.NameTemplate, out var entityName))
        {
            entityData.Name = entityName;
        }

        // Codes
        foreach (var entityCodeTemplate in context.EntityCodeTemplates)
        {
            if (data.TryGetValue(entityCodeTemplate.Value, out var value))
            {
                entityData.Codes.Add(
                    new EntityCode(
                        entityCodeTemplate.Type,
                        entityCodeTemplate.Origin,
                        value));
            }
        }

        // Properties
        foreach (var (key, value) in data)
        {
            entityData.Properties[$"{context.VocabularyPrefix}.{key}"] = value;
        }

        // Incoming Edges
        foreach (var entityEdgeTemplate in context.IncomingEntityEdgeTemplates)
        {
            var fromEntityCodeTemplate = entityEdgeTemplate.FromReference.Code;
            if (!data.TryGetValue(fromEntityCodeTemplate.Value, out var fromEntityCodeValue))
            {
                continue;
            }

            entityData.IncomingEdges.Add(
                new EntityEdge(
                    new EntityReference(
                        new EntityCode(
                            fromEntityCodeTemplate.Type,
                            fromEntityCodeTemplate.Origin,
                            fromEntityCodeValue)),
                    new EntityReference(clue.OriginEntityCode),
                    entityEdgeTemplate.EdgeType));
        }

        // Outgoing Edges
        foreach (var entityEdgeTemplate in context.OutgoingEntityEdgeTemplates)
        {
            var toEntityCodeTemplate = entityEdgeTemplate.ToReference.Code;
            if (!data.TryGetValue(toEntityCodeTemplate.Value, out var toEntityCodeValue))
            {
                continue;
            }

            entityData.OutgoingEdges.Add(
                new EntityEdge(
                    new EntityReference(clue.OriginEntityCode),
                    new EntityReference(
                        new EntityCode(
                            toEntityCodeTemplate.Type,
                            toEntityCodeTemplate.Origin,
                            toEntityCodeValue)),
                    entityEdgeTemplate.EdgeType));
        }

        return clue;
    }

    public static Dictionary<string, string> Flatten(this JsonElement jsonElement)
    {
        var result = new Dictionary<string, string>();

        FlattenJsonElement(jsonElement.Clone(), result, string.Empty);

        return result;
    }

    private static void FlattenJsonElement(JsonElement element, IDictionary<string, string> dict, string prefix)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var propName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    FlattenJsonElement(property.Value, dict, propName);
                }

                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    FlattenJsonElement(item, dict, $"{prefix}.{index}");
                    index++;
                }

                break;
            default:
                dict[prefix] = element.ToString();
                break;
        }
    }
}
