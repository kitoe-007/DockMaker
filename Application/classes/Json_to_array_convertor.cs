using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Autoaccept_electrical_app.classes
{
    /// <summary>
    /// Преобразование JSON от Python-экстрактора в двумерный массив строк по столбцам Excel.
    /// </summary>
    public class Json_to_array_convertor
    {
        /// <summary>
        /// Извлекает JSON-объект из stdout Python (отсекает лишний текст до «{» и после «}»).
        /// </summary>
        /// <param name="s">Сырой вывод процесса.</param>
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

        /// <summary>
        /// Десериализует JSON-строку в словарь полей.
        /// </summary>
        /// <param name="jsonString">JSON-текст от экстрактора.</param>
        static Dictionary<string, JsonElement> ConvertJsonToDictionary(string jsonString)
        {
            string clean = ExtractJsonObject(jsonString);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(clean)
                   ?? new Dictionary<string, JsonElement>();
        }

        /// <summary>
        /// Возвращает массив строк одного столбца по ключу column_N.
        /// </summary>
        /// <param name="dict">Словарь полей JSON.</param>
        /// <param name="key">Имя столбца (например, column_0).</param>
        static List<string> ConvertJsonDictElementToStringArray(Dictionary<string, JsonElement> dict, string key) {
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

        /// <summary>
        /// Конвертирует JSON экстрактора в список столбцов Excel.
        /// </summary>
        /// <param name="jsonString">Сырой вывод XLSX_extractor.py.</param>
        static public List<List<string>> ConvertJsonToStringArray(string jsonString)
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
