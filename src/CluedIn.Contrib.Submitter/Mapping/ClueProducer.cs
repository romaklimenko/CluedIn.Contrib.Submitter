using System.Xml;
using CluedIn.Core.Data;

namespace CluedIn.Contrib.Submitter.Mapping;

public static class ClueProducer
{
    public static Clue? ToClue(this Dictionary<string, string> data, Context context)
    {
        var sanitizedData = new Dictionary<string, string>();

        foreach (var (k, v) in data)
        {
            sanitizedData[k] = new string(v.Where(XmlConvert.IsXmlChar).ToArray()).Trim();
        }

        if (!sanitizedData.TryGetValue(context.OriginEntityCodeTemplate.Value, out var originEntityCodeValue))
        {
            context.Errors.Add(
                $"Can't get Origin Entity Code value of property '{context.OriginEntityCodeTemplate.Value}' " +
                $"from record {sanitizedData}.");
            return null;
        }

        var originEntityCode = new EntityCode(
            context.OriginEntityCodeTemplate.Type,
            context.OriginEntityCodeTemplate.Origin,
            originEntityCodeValue);

        var clue = new Clue(originEntityCode, context.OrganizationId);

        var entityData = clue.Data.EntityData;

        entityData.Codes.Add(originEntityCode);

        // Name
        if (!string.IsNullOrWhiteSpace(context.NameTemplate) &&
            sanitizedData.TryGetValue(context.NameTemplate, out var entityName))
        {
            entityData.Name = entityName;
        }

        // Codes
        foreach (var entityCodeTemplate in context.EntityCodeTemplates)
        {
            if (sanitizedData.TryGetValue(entityCodeTemplate.Value, out var value))
            {
                entityData.Codes.Add(
                    new EntityCode(
                        entityCodeTemplate.Type,
                        entityCodeTemplate.Origin,
                        value));
            }
        }

        // Properties
        foreach (var (key, value) in sanitizedData)
        {
            entityData.Properties[$"{context.VocabularyPrefix}.{key}"] = value;
        }

        // Incoming Edges
        foreach (var entityEdgeTemplate in context.IncomingEntityEdgeTemplates)
        {
            var fromEntityCodeTemplate = entityEdgeTemplate.FromReference.Code;
            if (!sanitizedData.TryGetValue(fromEntityCodeTemplate.Value, out var fromEntityCodeValue))
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
            if (!sanitizedData.TryGetValue(toEntityCodeTemplate.Value, out var toEntityCodeValue))
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
}
