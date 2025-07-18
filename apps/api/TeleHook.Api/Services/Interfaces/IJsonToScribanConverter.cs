using System.Text.Json;
using Scriban.Runtime;

namespace TeleHook.Api.Services.Interfaces;


public interface IJsonToScribanConverter
{
    ScriptObject ConvertToScriptObject(JsonElement element);

    ScriptObject ConvertToScriptObject(object data);
}
