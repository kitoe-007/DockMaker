using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Autoaccept_electrical_app.classes
{
    internal class Json_to_array_convertor
    {
        Dictionary<string, JsonElement> ConvertJsonToDictionary(string jsonString)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
            return dict;
        }

        List<string> ConvertJsonDictElementToStringArray(Dictionary<string, JsonElement> dict, string key) {
            if (dict.TryGetValue(key, out JsonElement columnElement))
            {
                
                if (columnElement.ValueKind == JsonValueKind.Array)
                {
                    List<string> items = columnElement.EnumerateArray()
                                                      .Select(e => e.ToString())
                                                      .ToList();
                    return items;
                }
            }
            return new List<string>();
        }

        List<List<string>> ConvertJsonToStringArray(string jsonString)
        {
            List<List<string>> result = new List<List<string>>();
            string key;
            Dictionary<string, JsonElement> dict = ConvertJsonToDictionary(jsonString);
            int columns = dict["number_of_columns"].GetInt32();
            for (int i = 0; i < columns; i++) { 
                key = $"column_{i}";
                result.Add(ConvertJsonDictElementToStringArray(dict, key));
            }
            return result;
        }
    }
}
