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
        Assert.Equal("Missing the mandatory 'entity_type' query string parameter.", errors[0]);
        Assert.Equal("Missing the mandatory 'origin_code' query string parameter.", errors[1]);
        Assert.Equal("Missing the mandatory 'vocab_prefix' query string parameter.", errors[2]);
    }

    [Fact]
    public void TryCreate_EntityType_ReturnsErrors()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
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
        Assert.Equal("Missing the mandatory 'origin_code' query string parameter.", errors[0]);
        Assert.Equal("Missing the mandatory 'vocab_prefix' query string parameter.", errors[1]);
    }

    [Fact]
    public void TryCreate_EntityTypeAndOriginCode_ReturnsErrors()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
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
        Assert.Equal("Missing the mandatory 'vocab_prefix' query string parameter.", errors[0]);
    }

    [Fact]
    public void TryCreate_Success()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
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
        Assert.Equal(EntityTypeConfigString, context.EntityTypeConfiguration);
        Assert.Equal(VocabPrefixConfigString, context.VocabularyPrefixConfiguration);
        Assert.Equal("Salesforce", context.OriginCodeConfiguration.Origin);
        Assert.Equal("id", context.OriginCodeConfiguration.Id);
    }

    [Fact]
    public void TryCreate_OneCode_CanParse()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
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
        Assert.Equal(EntityTypeConfigString, context.EntityTypeConfiguration);
        Assert.Equal("Salesforce", context.OriginCodeConfiguration.Origin);
        Assert.Equal("id", context.OriginCodeConfiguration.Id);

        Assert.Single(context.CodesConfiguration);
        Assert.Equal("Dynamics", context.CodesConfiguration[0].Origin);
        Assert.Equal("user_id", context.CodesConfiguration[0].Id);
    }

    [Fact]
    public void TryCreate_TwoCodes_CanParse()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
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
        Assert.Equal(EntityTypeConfigString, context.EntityTypeConfiguration);
        Assert.Equal("Salesforce", context.OriginCodeConfiguration.Origin);
        Assert.Equal("id", context.OriginCodeConfiguration.Id);

        Assert.Equal(2, context.CodesConfiguration.Count);
        Assert.Equal("Dynamics", context.CodesConfiguration[0].Origin);
        Assert.Equal("user_id", context.CodesConfiguration[0].Id);
        Assert.Equal("Sharepoint", context.CodesConfiguration[1].Origin);
        Assert.Equal("PersonId", context.CodesConfiguration[1].Id);
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
        Assert.Equal("Could not parse the Entity Code in the query string parameter's value " +
                     "The value must be in the format 'Origin:Id', " +
                     $"but found ''{codesConfigString}''", errors[0]);
    }

    [Fact]
    public void TryCreate_Edges_CanParse()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
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
        Assert.Equal(EntityTypeConfigString, context.EntityTypeConfiguration);
        Assert.Equal("Salesforce", context.OriginCodeConfiguration.Origin);
        Assert.Equal("id", context.OriginCodeConfiguration.Id);

        Assert.Equal(2, context.IncomingEdgesConfiguration.Count);
        Assert.Equal("/Manager", context.IncomingEdgesConfiguration[0].EdgeType);
        Assert.Equal("/Employee", context.IncomingEdgesConfiguration[0].EntityType);
        Assert.Equal("Sharepoint", context.IncomingEdgesConfiguration[0].Origin);
        Assert.Equal("ManagerId", context.IncomingEdgesConfiguration[0].Id);
        Assert.Equal("/Customer", context.IncomingEdgesConfiguration[1].EdgeType);
        Assert.Equal("/Contact", context.IncomingEdgesConfiguration[1].EntityType);
        Assert.Equal("CRM", context.IncomingEdgesConfiguration[1].Origin);
        Assert.Equal("id", context.IncomingEdgesConfiguration[1].Id);

        Assert.Equal(2, context.OutgoingEdgesConfiguration.Count);
        Assert.Equal("/Works", context.OutgoingEdgesConfiguration[0].EdgeType);
        Assert.Equal("/Organization", context.OutgoingEdgesConfiguration[0].EntityType);
        Assert.Equal("Salesforce", context.OutgoingEdgesConfiguration[0].Origin);
        Assert.Equal("org_id", context.OutgoingEdgesConfiguration[0].Id);
        Assert.Equal("/Manages", context.OutgoingEdgesConfiguration[1].EdgeType);
        Assert.Equal("/Employee", context.OutgoingEdgesConfiguration[1].EntityType);
        Assert.Equal("Sharepoint", context.OutgoingEdgesConfiguration[1].Origin);
        Assert.Equal("employee_id", context.OutgoingEdgesConfiguration[1].Id);
    }

    [Fact]
    public void TryCreate_BadEdgesConfig_ReturnsErrors()
    {
        // Arrange
        // Act
        var result = Context.TryCreate(
            QueryStringConfigString,
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
        Assert.Equal(2, errors.Count);
        Assert.Equal(
            "Could not parse entity edge configuration. " +
            "The value must be in the format '/EdgeType|/EntityType#Origin:id', " +
            "but found: '/WorksFor|Salesforce:org_id'",
            errors[0]);
        Assert.Equal(
            "Could not parse entity edge configuration. " +
            "The value must be in the format '/EdgeType|/EntityType#Origin:id', " +
            "but found: '/Works|/Organization#:org_id'",
            errors[1]);
    }
}
