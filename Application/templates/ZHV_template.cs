using Autoaccept_electrical_app.classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

namespace Autoaccept_electrical_app.templates
{
    /// <summary>
    /// Шаблон ЖВК — журнал входного учёта и контроля качества материалов.
    /// В отличие от ОЖР, каждая позиция из Excel даёт отдельную строку (без группировки по дате).
    /// </summary>
    internal class ZHV_template
    {
        // Индексы столбцов в JSON из XLSX_extractor (column_0, column_2, column_4)
        private const int ExcelColDate = 0;
        private const int ExcelColSection = 2;
        private const int ExcelColMaterialName = 4;

        private void set_headers(DataGrid dg)
        {
            if (dg.Columns.Count < 9) return;

            dg.Columns[0].Header = "№ п/п";
            dg.Columns[1].Header = "Дата доставки";
            dg.Columns[2].Header = "Наименование материалов и конструкций";
            dg.Columns[3].Header = "Кол-во";
            dg.Columns[4].Header = "Поставщик";
            dg.Columns[5].Header = "Наименование и номер сопроводительного документа\n(сертификаты, декларации, паспорта качества)";
            dg.Columns[6].Header = "Вид контроля\n(визуальный, инструментальный, лабораторный)";
            dg.Columns[7].Header = "Результат контроля";
            dg.Columns[8].Header = "Подпись лица, осуществляющего контроль";
        }

        private void clear_table(DataGrid dg)
        {
            dg.Columns.Clear();
            dg.ItemsSource = null;
        }

        // Сравнение разделов без учёта регистра, пробелов и дефисов (как в OJR_template)
        private static string NormalizeSection(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            return name.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        }

        // Фильтр «Раздел» поддерживает несколько значений через запятую: «АК-1, ЭМ-5»
        private static bool SectionMatches(string currentSection, string sectionFilter)
        {
            var current = NormalizeSection(currentSection);
            if (string.IsNullOrEmpty(current))
                return false;

            var allowed = sectionFilter
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeSection)
                .Where(s => !string.IsNullOrEmpty(s));

            return allowed.Any(s => s == current);
        }

        private static string GetCell(List<List<string>> data, int columnIndex, int rowIndex)
        {
            if (data == null || columnIndex < 0 || columnIndex >= data.Count)
                return "";

            var column = data[columnIndex];
            if (column == null || rowIndex < 0 || rowIndex >= column.Count)
                return "";

            return column[rowIndex] ?? "";
        }

        /// <param name="sectionFilter">Раздел из UI; пустая строка — без фильтрации</param>
        /// <param name="controlType">Вид контроля по умолчанию для всех строк</param>
        /// <param name="controlResult">Результат контроля по умолчанию</param>
        /// <param name="inspectorFio">Подпись лица, осуществляющего контроль</param>
        public void create_table(
            DataGrid dg,
            List<List<string>> data,
            int cut_top,
            int cut_bottom,
            FileClass current_file,
            string sectionFilter,
            string controlType,
            string controlResult,
            string inspectorFio)
        {
            clear_table(dg);

            if (data == null || data.Count < 5 || data[0] == null || data[4] == null || data[0].Count < 2)
                return;

            bool useSectionFilter = !string.IsNullOrWhiteSpace(sectionFilter);
            string defaultControlType = string.IsNullOrWhiteSpace(controlType) ? "Визуальный" : controlType.Trim();
            string defaultControlResult = string.IsNullOrWhiteSpace(controlResult) ? "Пригоден" : controlResult.Trim();
            string defaultInspectorFio = inspectorFio?.Trim() ?? "";

            var table = new DataTable();
            for (int c = 0; c < 9; c++)
                table.Columns.Add($"column_{c}", typeof(string));

            int ind = 0;
            // «Текущие» дата и раздел наследуются вниз по листу, пока не встретится новое значение
            string currentSection = "";
            string currentDate = "";
            bool inMatchingSection = !useSectionFilter;

            // Строка 0 в Excel — заголовок; cut_top/cut_bottom обрезают лишние строки
            int start = 1 + cut_top;
            int endExclusive = data[0].Count - cut_bottom;
            if (start < 1) start = 1;
            if (endExclusive < start) endExclusive = start;

            for (int i = start; i < endExclusive; i++)
            {
                string dateVal = GetCell(data, ExcelColDate, i);
                string sectionVal = GetCell(data, ExcelColSection, i);
                string nameVal = GetCell(data, ExcelColMaterialName, i);

                if (!string.IsNullOrEmpty(dateVal))
                {
                    currentDate = dateVal;

                    if (!string.IsNullOrEmpty(sectionVal))
                        currentSection = sectionVal;

                    inMatchingSection = !useSectionFilter || SectionMatches(currentSection, sectionFilter);
                }
                else if (!string.IsNullOrEmpty(sectionVal))
                {
                    currentSection = sectionVal;
                    inMatchingSection = !useSectionFilter || SectionMatches(currentSection, sectionFilter);
                }

                if (!inMatchingSection || string.IsNullOrEmpty(nameVal))
                    continue;

                // Дата может быть только в «якорной» строке; для продолжений берём currentDate
                string rowDate = !string.IsNullOrEmpty(dateVal) ? dateVal : currentDate;
                if (string.IsNullOrEmpty(rowDate))
                    continue;

                ind++;
                var row = table.NewRow();
                row[0] = ind.ToString();
                row[1] = rowDate;
                row[2] = nameVal;
                // Кол-во, поставщик и сопроводительный документ в Excel не заполняются
                row[3] = "";
                row[4] = "";
                row[5] = "";
                row[6] = defaultControlType;
                row[7] = defaultControlResult;
                row[8] = defaultInspectorFio;
                table.Rows.Add(row);
            }

            dg.AutoGenerateColumns = true;
            dg.ItemsSource = table.DefaultView;
            set_headers(dg);
            current_file.table_data = table;
        }
    }
}
