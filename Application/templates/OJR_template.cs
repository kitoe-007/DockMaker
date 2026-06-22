using Autoaccept_electrical_app.classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

namespace Autoaccept_electrical_app.templates
{
    internal class OJR_template
    {
        public string Name { get; set; }

        private void set_headers(DataGrid dg)
        {
            if (dg.Columns.Count < 4) return;

            dg.Columns[0].Header = "№/пп";
            dg.Columns[1].Header = "Дата выполнения работ";
            dg.Columns[2].Header = "Наименование работ, выполняемых в процессе строительства,\n реконструкции, капитального ремонта объекта капитального строительства";
            dg.Columns[3].Header = "Должность, фамилия, инициалы, подпись уполномоченного\nпредставителя лица, осуществляющего строительство";
        }

        private void clear_table(DataGrid dg)
        {
            dg.Columns.Clear();
            dg.ItemsSource = null;
        }

        private static string NormalizeSection(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            return name.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        }

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

        private static void AppendColumnValue(DataRow row, int columnIndex, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            string prev = row[columnIndex]?.ToString() ?? "";
            row[columnIndex] = string.IsNullOrEmpty(prev) ? value : prev + ",\n " + value;
        }

        private static void FlushRow(DataTable table, ref DataRow currentRow)
        {
            if (currentRow == null)
                return;

            table.Rows.Add(currentRow);
            currentRow = null;
        }

        public void create_table(DataGrid dg, List<List<string>> data, int cut_top, int cut_bottom, FileClass current_file, string sectionFilter, string positionFio)
        {
            clear_table(dg);

            if (data == null || data.Count < 5 || data[0] == null || data[4] == null || data[0].Count < 2)
                return;

            bool useSectionFilter = !string.IsNullOrWhiteSpace(sectionFilter);
            bool usePositionFioFromField = !string.IsNullOrWhiteSpace(positionFio);

            var table = new DataTable();
            table.Columns.Add("column_0", typeof(string));
            table.Columns.Add("column_1", typeof(string));
            table.Columns.Add("column_2", typeof(string));
            table.Columns.Add("column_3", typeof(string));

            int ind = 0;
            DataRow currentRow = null;
            string currentSection = "";
            string currentDate = "";
            bool inMatchingSection = !useSectionFilter;

            int start = 1 + cut_top;
            int endExclusive = data[0].Count - cut_bottom;
            if (start < 1) start = 1;
            if (endExclusive < start) endExclusive = start;

            for (int i = start; i < endExclusive; i++)
            {
                string dateVal = GetCell(data, 0, i);
                string sectionVal = GetCell(data, 2, i);
                string nameVal = GetCell(data, 4, i);

                if (!string.IsNullOrEmpty(dateVal))
                {
                    FlushRow(table, ref currentRow);
                    currentDate = dateVal;

                    if (!string.IsNullOrEmpty(sectionVal))
                        currentSection = sectionVal;

                    inMatchingSection = !useSectionFilter || SectionMatches(currentSection, sectionFilter);
                    if (!inMatchingSection)
                        continue;

                    ind++;
                    currentRow = table.NewRow();
                    currentRow[0] = ind.ToString();
                    currentRow[1] = dateVal;
                    currentRow[2] = "";
                    currentRow[3] = usePositionFioFromField ? positionFio.Trim() : "";
                }
                else if (!string.IsNullOrEmpty(sectionVal))
                {
                    bool wasMatching = inMatchingSection;
                    currentSection = sectionVal;
                    inMatchingSection = !useSectionFilter || SectionMatches(currentSection, sectionFilter);

                    if (wasMatching && !inMatchingSection)
                        FlushRow(table, ref currentRow);
                }

                if (!inMatchingSection)
                    continue;

                if (currentRow == null && !string.IsNullOrEmpty(currentDate) &&
                    !string.IsNullOrEmpty(nameVal))
                {
                    ind++;
                    currentRow = table.NewRow();
                    currentRow[0] = ind.ToString();
                    currentRow[1] = currentDate;
                    currentRow[2] = "";
                    currentRow[3] = usePositionFioFromField ? positionFio.Trim() : "";
                }

                if (currentRow == null)
                    continue;

                if (!string.IsNullOrEmpty(nameVal))
                    AppendColumnValue(currentRow, 2, nameVal);
            }

            FlushRow(table, ref currentRow);

            dg.AutoGenerateColumns = true;
            dg.ItemsSource = table.DefaultView;
            set_headers(dg);
            current_file.table_data = table;
        }
    }
}
