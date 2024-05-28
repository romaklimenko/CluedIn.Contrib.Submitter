using CluedIn.Contrib.Submitter.Helpers;

namespace CluedIn.Contrib.Submitter.Tests.Helpers;

public class StringExtensionsTest
{
    [Theory]
    [InlineData("Dynamics365", "63019bf7-0df8-5bcd-9837-87cbe4e2687f")]
    [InlineData("Salesforce", "d937cb41-085f-54d9-b582-cc1e14e6f109")]
    [InlineData("Marketo", "d486868c-03a7-5e8e-a9c8-e7dcf8e84fcc")]
    [InlineData("Virtuous", "c524bec7-06fd-5c35-ae5b-2ce23095e407")]
    public void ToGuid_GeneratesGuid(string input, string expected)
    {
        // Arrange
        // Act
        var actual = input.ToGuid();
        // Assert
        Assert.Equal(new Guid(expected), actual);
    }

    [Fact]
    public void ToGuid_Null()
    {
        // Arrange
        string? input = null;
        // Act
        // Assert
        var exception = Assert.Throws<ArgumentNullException>(() => input!.ToGuid());
        Assert.Equal("input", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public void ToGuid_EmptyOrWhitespace(string input)
    {
        // Arrange
        // Act
        // Assert
        var exception = Assert.Throws<ArgumentException>(() => input.ToGuid());
        Assert.Equal("input", exception.ParamName);
    }
}
