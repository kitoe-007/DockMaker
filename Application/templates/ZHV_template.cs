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
    /// </summary>
    internal class ZHV_template
    {
        private const int ExcelColDate = 0;
        private const int ExcelColSection = 2;
        private const int ExcelColMaterialName = 4;

        /// <summary>
        /// Задаёт русские заголовки столбцов таблицы ЖВК.
        /// </summary>
        /// <param name="dg">Сетка предпросмотра документа.</param>
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

        /// <summary>
        /// Очищает DataGrid перед построением новой таблицы.
        /// </summary>
        /// <param name="dg">Сетка предпросмотра документа.</param>
        private void clear_table(DataGrid dg)
        {
            dg.Columns.Clear();
            dg.ItemsSource = null;
        }

        /// <summary>
        /// Нормализует код раздела для сравнения (без регистра, пробелов и дефисов).
        /// </summary>
        /// <param name="name">Исходный код раздела.</param>
        private static string NormalizeSection(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            return name.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        }

        /// <summary>
        /// Проверяет, входит ли текущий раздел в фильтр (несколько значений через запятую).
        /// </summary>
        /// <param name="currentSection">Текущий раздел строки Excel.</param>
        /// <param name="sectionFilter">Строка фильтра разделов.</param>
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

        /// <summary>
        /// Безопасно читает ячейку из столбцового представления данных Excel.
        /// </summary>
        /// <param name="data">Данные Excel в столбцовом виде.</param>
        /// <param name="columnIndex">Индекс столбца.</param>
        /// <param name="rowIndex">Индекс строки.</param>
        private static string GetCell(List<List<string>> data, int columnIndex, int rowIndex)
        {
            if (data == null || columnIndex < 0 || columnIndex >= data.Count)
                return "";

            var column = data[columnIndex];
            if (column == null || rowIndex < 0 || rowIndex >= column.Count)
                return "";

            return column[rowIndex] ?? "";
        }

        /// <summary>
        /// Строит таблицу ЖВК из данных Excel и отображает её в DataGrid.
        /// </summary>
        /// <param name="dg">Сетка предпросмотра документа.</param>
        /// <param name="data">Данные Excel в столбцовом виде.</param>
        /// <param name="cut_top">Число строк обрезки сверху.</param>
        /// <param name="cut_bottom">Число строк обрезки снизу.</param>
        /// <param name="current_file">Объект состояния текущего документа.</param>
        /// <param name="sectionFilter">Фильтр по разделу (пустой — без фильтра).</param>
        /// <param name="supplier">Поставщик по умолчанию для строк.</param>
        /// <param name="controlType">Вид контроля по умолчанию.</param>
        /// <param name="controlResult">Результат контроля по умолчанию.</param>
        /// <param name="inspectorFio">ФИО контролирующего лица.</param>
        public void create_table(
            DataGrid dg,
            List<List<string>> data,
            int cut_top,
            int cut_bottom,
            FileClass current_file,
            string sectionFilter,
            string supplier,
            string controlType,
            string controlResult,
            string inspectorFio)
        {
            clear_table(dg);

            if (data == null || data.Count < 5 || data[0] == null || data[4] == null || data[0].Count < 2)
                return;

            bool useSectionFilter = !string.IsNullOrWhiteSpace(sectionFilter);
            string defaultSupplier = supplier?.Trim() ?? "";
            string defaultControlType = string.IsNullOrWhiteSpace(controlType) ? "Визуальный" : controlType.Trim();
            string defaultControlResult = string.IsNullOrWhiteSpace(controlResult) ? "Пригоден" : controlResult.Trim();
            string defaultInspectorFio = inspectorFio?.Trim() ?? "";

            var table = new DataTable();
            for (int c = 0; c < 9; c++)
                table.Columns.Add($"column_{c}", typeof(string));

            int ind = 0;
            // дата и раздел идут вниз по листу, пока не встретится новое значение
            string currentSection = "";
            string currentDate = "";
            bool inMatchingSection = !useSectionFilter;

            // строки 1..N: cut_top/cut_bottom обрезают лишние строки сверху и снизу
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

                string rowDate = !string.IsNullOrEmpty(dateVal) ? dateVal : currentDate;
                if (string.IsNullOrEmpty(rowDate))
                    continue;

                ind++;
                var row = table.NewRow();
                row[0] = ind.ToString();
                row[1] = rowDate;
                row[2] = nameVal;

                row[3] = "";
                row[4] = defaultSupplier;
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
