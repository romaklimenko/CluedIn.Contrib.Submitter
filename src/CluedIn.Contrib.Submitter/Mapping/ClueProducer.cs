using CluedIn.Core.Data;

namespace CluedIn.Contrib.Submitter.Mapping;

public static class ClueProducer
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

        var originEntityCode = new EntityCode(
            context.OriginEntityCodeTemplate.Type,
            context.OriginEntityCodeTemplate.Origin,
            originEntityCodeValue);

        var clue = new Clue(originEntityCode, context.OrganizationId);

        var entityData = clue.Data.EntityData;

        entityData.Codes.Add(originEntityCode);

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
}
