using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Windows.Controls;

namespace Autoaccept_electrical_app.classes
{
    /// <summary>
    /// Сериализация DataTable в JSON для передачи в Python-сборщик DOCX.
    /// </summary>
    internal class DataTable_to_json
    {
        /// <summary>
        /// Сериализует таблицу без заголовков столбцов.
        /// </summary>
        /// <param name="data">Таблица данных для экспорта.</param>
        public string ConvertDataTableToJson(DataTable data)
        {
            return JsonSerializer.Serialize(convert_datatable_to_dict(data, null));
        }

        /// <summary>
        /// Сериализует таблицу с заголовками из DataGrid.
        /// </summary>
        /// <param name="data">Таблица данных для экспорта.</param>
        /// <param name="dataGrid">Сетка с заголовками столбцов.</param>
        public string ConvertDataTableToJson(DataTable data, DataGrid dataGrid)
        {
            return ConvertDataTableToJson(data, dataGrid, null);
        }

        /// <summary>
        /// Сериализует таблицу с заголовками и служебными полями шапки (ключи с «_»).
        /// </summary>
        /// <param name="data">Таблица данных для экспорта.</param>
        /// <param name="dataGrid">Сетка с заголовками столбцов.</param>
        /// <param name="metadata">Служебные поля шапки DOCX (ключи с «_»).</param>
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

        /// <summary>
        /// Преобразует DataTable в словарь column_N → список значений (первый элемент — заголовок).
        /// </summary>
        /// <param name="data">Исходная таблица.</param>
        /// <param name="columnHeaders">Заголовки столбцов или null.</param>
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

        /// <summary>
        /// Собирает значения одного столбца таблицы в список строк.
        /// </summary>
        /// <param name="data">Исходная таблица.</param>
        /// <param name="column">Столбец, значения которого собираются.</param>
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
