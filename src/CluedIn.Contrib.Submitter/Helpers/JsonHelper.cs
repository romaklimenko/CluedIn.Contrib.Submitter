using System.Text.Json;

namespace CluedIn.Contrib.Submitter.Helpers;

public static class JsonHelper
{
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
