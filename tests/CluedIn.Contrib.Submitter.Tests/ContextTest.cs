namespace CluedIn.Contrib.Submitter.Tests;

public class ContextTest
{
    // Valid parameters
    private const string QueryStringConfigString = "?foo=bar";
    private const string EntityTypeConfigString = "/Person";
    private const string OriginCodeConfigString = "Salesforce:id";
    private const string VocabPrefixConfigString = "Salesforce:id";
    private const string CodesConfigString = "Dynamics:user_id,Sharepoint:PersonId";

    private const string IncomingEdgesConfigString = "/Manager|/Employee#Sharepoint:ManagerId," +
                                                     "/Customer|/Contact#CRM:id,";

    private const string OutgoingEdgesConfigString = "/Works|/Organization#Salesforce:org_id," +
                                                     "/Manages|/Employee#Sharepoint:employee_id,";

    [Fact]
    public void TryCreate_WrongParameters_ReturnsErrors()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            out var context,
            out var errors);
        // Assert
        Assert.False(result);
        Assert.Null(context);
        Assert.Equal(3, errors.Count);
        Assert.Equal("Entity Type must not be null or empty string.", errors[0]);
        Assert.Equal("Can't parse Entity Code if Entity Type is null or empty string.", errors[1]);
        Assert.Equal("Vocabulary Prefix must not be null or empty string.", errors[2]);
    }

    [Fact]
    public void TryCreate_EntityType_ReturnsErrors()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
            Guid.NewGuid(),
            null,
            EntityTypeConfigString,
            null,
            null,
            null,
            null,
            null,
            out var context,
            out var errors);
        // Assert
        Assert.False(result);
        Assert.Null(context);
        Assert.Equal(2, errors.Count);
        Assert.Equal(
            "Error when parsing Entity Code. Provided value: '/Person#'. " +
            "Exception: Invalid entityCodeKey: /Person# (Parameter 'entityCodeKey').",
            errors[0]);
        Assert.Equal("Vocabulary Prefix must not be null or empty string.", errors[1]);
    }

    [Fact]
    public void TryCreate_EntityTypeAndOriginCode_ReturnsErrors()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
            Guid.NewGuid(),
            null,
            EntityTypeConfigString,
            OriginCodeConfigString,
            null,
            null,
            null,
            null,
            out var context,
            out var errors);
        // Assert
        Assert.False(result);
        Assert.Null(context);
        Assert.Single(errors);
        Assert.Equal("Vocabulary Prefix must not be null or empty string.", errors[0]);
    }

    [Fact]
    public void TryCreate_Success()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
            Guid.NewGuid(),
            null,
            EntityTypeConfigString,
            OriginCodeConfigString,
            VocabPrefixConfigString,
            null,
            null,
            null,
            out var context,
            out var errors);
        // Assert
        Assert.True(result);
        Assert.NotNull(context);
        Assert.Empty(errors);
        Assert.Equal(QueryStringConfigString, context.QueryString);
        Assert.Equal(EntityTypeConfigString, context.EntityType);
        Assert.Equal(VocabPrefixConfigString, context.VocabularyPrefix);
        Assert.Equal("Salesforce", context.OriginEntityCodeTemplate.Origin);
        // Assert.Equal("id", context.OriginEntityCode.Id);
    }

    [Fact]
    public void TryCreate_OneCode_CanParse()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
            Guid.NewGuid(),
            null,
            EntityTypeConfigString,
            OriginCodeConfigString,
            VocabPrefixConfigString,
            "Dynamics:user_id",
            null,
            null,
            out var context,
            out var errors);
        // Assert
        Assert.True(result);
        Assert.NotNull(context);
        Assert.Empty(errors);
        Assert.Equal(QueryStringConfigString, context.QueryString);
        Assert.Equal(EntityTypeConfigString, context.EntityType);
        Assert.Equal("Salesforce", context.OriginEntityCodeTemplate.Origin);
        Assert.Equal("id", context.OriginEntityCodeTemplate.Value);

        Assert.Single(context.EntityCodeTemplates);
        Assert.Equal("Dynamics", context.EntityCodeTemplates[0].Origin);
        Assert.Equal("user_id", context.EntityCodeTemplates[0].Value);
    }

    [Fact]
    public void TryCreate_TwoCodes_CanParse()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
            Guid.NewGuid(),
            null,
            EntityTypeConfigString,
            OriginCodeConfigString,
            VocabPrefixConfigString,
            CodesConfigString,
            null,
            null,
            out var context,
            out var errors);
        // Assert
        Assert.True(result);
        Assert.NotNull(context);
        Assert.Empty(errors);
        Assert.Equal(QueryStringConfigString, context.QueryString);
        Assert.Equal(EntityTypeConfigString, context.EntityType);
        Assert.Equal("Salesforce", context.OriginEntityCodeTemplate.Origin);
        Assert.Equal("id", context.OriginEntityCodeTemplate.Value);

        Assert.Equal(2, context.EntityCodeTemplates.Count);
        Assert.Equal("Dynamics", context.EntityCodeTemplates[0].Origin);
        Assert.Equal("user_id", context.EntityCodeTemplates[0].Value);
        Assert.Equal("Sharepoint", context.EntityCodeTemplates[1].Origin);
        Assert.Equal("PersonId", context.EntityCodeTemplates[1].Value);
    }

    [Theory]
    [InlineData("Dynamics:")]
    [InlineData(":")]
    [InlineData(":id")]
    public void TryCreate_BadCodesConfig_ReturnsErrors(string codesConfigString)
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
            Guid.NewGuid(),
            null,
            EntityTypeConfigString,
            OriginCodeConfigString,
            VocabPrefixConfigString,
            codesConfigString,
            null,
            null,
            out var context,
            out var errors);
        // Assert
        Assert.False(result);
        Assert.Null(context);
        Assert.Single(errors);
        Assert.Equal(
            "Error when parsing Entity Code. " +
            $"Provided value: '{EntityTypeConfigString}#{codesConfigString}'. Exception: Invalid entityCodeKey: {EntityTypeConfigString}#{codesConfigString} (Parameter 'entityCodeKey').",
            errors[0]);
    }

    [Fact]
    public void TryCreate_Edges_CanParse()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
            Guid.NewGuid(),
            null,
            EntityTypeConfigString,
            OriginCodeConfigString,
            VocabPrefixConfigString,
            null,
            IncomingEdgesConfigString,
            OutgoingEdgesConfigString,
            out var context,
            out var errors);
        // Assert
        Assert.True(result);
        Assert.NotNull(context);
        Assert.Empty(errors);
        Assert.Equal(QueryStringConfigString, context.QueryString);
        Assert.Equal(EntityTypeConfigString, context.EntityType);
        Assert.Equal("Salesforce", context.OriginEntityCodeTemplate.Origin);
        Assert.Equal("id", context.OriginEntityCodeTemplate.Value);

        Assert.All(context.IncomingEntityEdgeTemplates,
            x => Assert.Equal(context.OriginEntityCodeTemplate, x.ToReference.Code));
        Assert.All(context.OutgoingEntityEdgeTemplates,
            x => Assert.Equal(context.OriginEntityCodeTemplate, x.FromReference.Code));

        Assert.Equal(2, context.IncomingEntityEdgeTemplates.Count);
        Assert.Equal("/Manager", context.IncomingEntityEdgeTemplates[0].EdgeType);
        Assert.Equal("/Employee", context.IncomingEntityEdgeTemplates[0].FromReference.Code.Type);
        Assert.Equal("Sharepoint", context.IncomingEntityEdgeTemplates[0].FromReference.Code.Origin);
        Assert.Equal("ManagerId", context.IncomingEntityEdgeTemplates[0].FromReference.Code.Value);
        Assert.Equal("/Customer", context.IncomingEntityEdgeTemplates[1].EdgeType);
        Assert.Equal("/Contact", context.IncomingEntityEdgeTemplates[1].FromReference.Code.Type);
        Assert.Equal("CRM", context.IncomingEntityEdgeTemplates[1].FromReference.Code.Origin);
        Assert.Equal("id", context.IncomingEntityEdgeTemplates[1].FromReference.Code.Value);

        Assert.Equal(2, context.OutgoingEntityEdgeTemplates.Count);
        Assert.Equal("/Works", context.OutgoingEntityEdgeTemplates[0].EdgeType);
        Assert.Equal("Salesforce", context.OutgoingEntityEdgeTemplates[0].ToReference.Code.Origin);
        Assert.Equal("org_id", context.OutgoingEntityEdgeTemplates[0].ToReference.Code.Value);
        Assert.Equal("/Manages", context.OutgoingEntityEdgeTemplates[1].EdgeType);
        Assert.Equal("/Employee", context.OutgoingEntityEdgeTemplates[1].ToReference.Code.Type);
        Assert.Equal("Sharepoint", context.OutgoingEntityEdgeTemplates[1].ToReference.Code.Origin);
        Assert.Equal("employee_id", context.OutgoingEntityEdgeTemplates[1].ToReference.Code.Value);
    }

    [Fact]
    public void TryCreate_BadEdgesConfig_ReturnsErrors()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
            Guid.NewGuid(),
            null,
            EntityTypeConfigString,
            OriginCodeConfigString,
            VocabPrefixConfigString,
            null,
            "/WorksFor|Salesforce:org_id",
            "/Works|/Organization#:org_id",
            out var context,
            out var errors);
        // Assert
        Assert.False(result);
        Assert.Null(context);
        Assert.Equal(4, errors.Count);
        Assert.Equal(
            "Error when parsing Entity Code. Provided value: 'Salesforce:org_id'. Exception: Invalid entityCodeKey: Salesforce:org_id (Parameter 'entityCodeKey').",
            errors[0]);
        Assert.Equal(
            "Could not parse '/WorksFor|Salesforce:org_id' into an Entity Edge.",
            errors[1]);
        Assert.Equal(
            "Error when parsing Entity Code. Provided value: '/Organization#:org_id'. Exception: Invalid entityCodeKey: /Organization#:org_id (Parameter 'entityCodeKey').",
            errors[2]);
        Assert.Equal(
            "Could not parse '/Works|/Organization#:org_id' into an Entity Edge.",
            errors[3]);
    }
}
