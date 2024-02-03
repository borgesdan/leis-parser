using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library
{
    public class LeiSerializer
    {
        public static string? ToJson(LeiNode leiNode)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var json = JsonSerializer.Serialize(leiNode, options);
            return json;
        }
    }
}
