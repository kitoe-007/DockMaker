using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;

namespace Autoaccept_electrical_app.classes
{
    internal class DataTable_to_json
    {
        

        public string ConvertDataTableToJson(DataTable data)
        {
            var dict = convert_datatable_to_dict(data);
            return JsonSerializer.Serialize(dict);
        }

        private Dictionary<string, List<string>> convert_datatable_to_dict(DataTable data)
        {
            var dict = new Dictionary<string, List<string>>();
            if (data == null)
                return dict;

            foreach (DataColumn column in data.Columns)
            {
                dict[column.ColumnName] = GetColumnValues(data, column);
            }

            return dict;
        }

        private static List<string> GetColumnValues(DataTable data, DataColumn column)
        {
            var values = new List<string>(data.Rows.Count);
            foreach (DataRow row in data.Rows)
            {
                values.Add(row[column]?.ToString() ?? "");
            }
            return values;
        }
    }
}
