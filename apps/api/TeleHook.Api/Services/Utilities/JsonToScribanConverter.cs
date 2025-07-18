using System.Text.Json;
using Scriban.Runtime;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Utilities;

/// <summary>
/// Service for converting JSON data to Scriban-compatible objects.
/// </summary>
public class JsonToScribanConverter : IJsonToScribanConverter
{
    public ScriptObject ConvertToScriptObject(JsonElement element)
    {
        var scriptObject = new ScriptObject();
        ConvertJsonElementToScriptObjectRecursive(element, scriptObject);
        return scriptObject;
    }

    public ScriptObject ConvertToScriptObject(object data)
    {
        var jsonElement = ConvertToJsonElement(data);
        return ConvertToScriptObject(jsonElement);
    }

    private JsonElement ConvertToJsonElement(object data)
    {
        if (data is JsonElement element)
            return element;

        var json = JsonSerializer.Serialize(data);
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    private void ConvertJsonElementToScriptObjectRecursive(JsonElement element, ScriptObject target)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        var nestedObject = new ScriptObject();
                        ConvertJsonElementToScriptObjectRecursive(property.Value, nestedObject);
                        target[property.Name] = nestedObject;
                    }
                    else if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        var array = new ScriptArray();
                        foreach (var item in property.Value.EnumerateArray())
                        {
                            array.Add(ConvertJsonElementToValue(item));
                        }
                        target[property.Name] = array;
                    }
                    else
                    {
                        target[property.Name] = ConvertJsonElementToValue(property.Value);
                    }
                }
                break;

            case JsonValueKind.Array:
                // This case is handled in the Object case above when converting arrays
                break;
        }
    }

    private object? ConvertJsonElementToValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.Number when element.TryGetInt64(out var longValue) => longValue,
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => ConvertToScriptObject(element),
            JsonValueKind.Array => ConvertJsonArrayToScriptArray(element),
            _ => element.ToString()
        };
    }

    private ScriptArray ConvertJsonArrayToScriptArray(JsonElement arrayElement)
    {
        var scriptArray = new ScriptArray();
        foreach (var item in arrayElement.EnumerateArray())
        {
            scriptArray.Add(ConvertJsonElementToValue(item));
        }
        return scriptArray;
    }
}
