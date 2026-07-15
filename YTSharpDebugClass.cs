using System.Text.Json;
using System.Text.Json.Nodes;

namespace DebugUtility
{

    internal class YTSharpDebugClass
    {

        internal static void WriteJsonToTestFile(JsonObject data, int fileNumber = 1)
        {

            if (data != null)
            {
                string path = $"./test{fileNumber}.json";

                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);

                string content = JsonSerializer.Serialize(data, jsonSerializerOptions);

                File.WriteAllText(path, content);

            }



        }
        internal static void WriteJsonToTestFile(JsonArray data, int fileNumber = 1)
        {

            if (data != null)
            {
                string path = $"./test{fileNumber}.json";

                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);

                string content = JsonSerializer.Serialize(data, jsonSerializerOptions);

                File.WriteAllText(path, content);
            }

        }

    }

}
