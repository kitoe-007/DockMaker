using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Windows.Controls;

namespace Autoaccept_electrical_app.classes
{
    internal class DataTable_to_json
    {
        public string ConvertDataTableToJson(DataTable data)
        {
            return JsonSerializer.Serialize(convert_datatable_to_dict(data, null));
        }

        public string ConvertDataTableToJson(DataTable data, DataGrid dataGrid)
        {
            return ConvertDataTableToJson(data, dataGrid, null);
        }

        public string ConvertDataTableToJson(DataTable data, DataGrid dataGrid, IDictionary<string, string> metadata)
        {
            IList<string> headers = null;
            if (dataGrid != null)
            {
                headers = dataGrid.Columns
                    .Cast<DataGridColumn>()
                    .OrderBy(c => c.DisplayIndex)
                    .Select(c => c.Header?.ToString() ?? "")
                    .ToList();
            }

            var dict = convert_datatable_to_dict(data, headers);
            // metadata: ключи с «_» — служебные поля шапки DOCX, не столбцы таблицы
            if (metadata != null)
            {
                foreach (var entry in metadata)
                    dict[entry.Key] = new List<string> { entry.Value ?? "" };
            }

            return JsonSerializer.Serialize(dict);
        }

        private Dictionary<string, List<string>> convert_datatable_to_dict(DataTable data, IList<string> columnHeaders)
        {
            var dict = new Dictionary<string, List<string>>();
            if (data == null)
                return dict;

            foreach (DataColumn column in data.Columns)
            {
                var values = GetColumnValues(data, column);

                if (columnHeaders != null && column.Ordinal < columnHeaders.Count)
                    values.Insert(0, columnHeaders[column.Ordinal]);

                dict[column.ColumnName] = values;
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
