using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Autoaccept_electrical_app.classes
{
    public class Json_to_array_convertor
    {
        static string ExtractJsonObject(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return "";

            int first = s.IndexOf('{');
            int last = s.LastIndexOf('}');
            if (first < 0 || last < 0 || last <= first)
                return s.Trim();

            return s.Substring(first, last - first + 1).Trim();
        }

        Dictionary<string, JsonElement> ConvertJsonToDictionary(string jsonString)
        {
            string clean = ExtractJsonObject(jsonString);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(clean)
                   ?? new Dictionary<string, JsonElement>();
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

        public List<List<string>> ConvertJsonToStringArray(string jsonString)
        {
            try
            {
                List<List<string>> result = new List<List<string>>();
                Dictionary<string, JsonElement> dict = ConvertJsonToDictionary(jsonString);

                if (!dict.TryGetValue("number_of_columns", out var colsElement) || colsElement.ValueKind != JsonValueKind.Number)
                    return result;

                int columns = colsElement.GetInt32();
                for (int i = 0; i < columns; i++)
                {
                    string key = $"column_{i}";
                    result.Add(ConvertJsonDictElementToStringArray(dict, key));
                }
                return result;
            }
            catch (JsonException)
            {
                return new List<List<string>>();
            }
            catch (Exception)
            {
                return new List<List<string>>();
            }
        }
    }
}
