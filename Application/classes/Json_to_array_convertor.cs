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
        Dictionary<string, object> ConvertJsonToDictionary(string jsonString)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
            return dict;
        }
    }
}
