namespace CluedIn.Contrib.Submitter.Helpers;

public static class EnvironmentHelper
{
    public static string GetStringEnvironmentVariable(string environmentVariableName, string fallbackValue)
    {
        var environmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableName);
        return string.IsNullOrWhiteSpace(environmentVariableValue)
            ? fallbackValue
            : environmentVariableValue;
    }

    public static int GetIntegerEnvironmentVariable(string environmentVariableName, int fallbackValue)
    {
        var environmentVariableStringValue = Environment.GetEnvironmentVariable(environmentVariableName);
        if (!string.IsNullOrWhiteSpace(environmentVariableStringValue) &&
            int.TryParse(environmentVariableStringValue, out var environmentVariableValue))
        {
            return environmentVariableValue;
        }

        return fallbackValue;
    }
}
