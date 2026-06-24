using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Autoaccept_electrical_app.classes.validation
{
    /// <summary>
    /// Вспомогательные методы нормализации и разбора фильтра разделов Excel.
    /// </summary>
    internal static class SectionValidationHelper
    {
        private const int ExcelColSection = 2;

        private static readonly Regex SectionTokenPattern = new Regex(
            @"^[A-Za-zА-Яа-яЁё0-9\-]+$",
            RegexOptions.Compiled);

        /// <summary>
        /// Приводит код раздела к единому виду для сравнения.
        /// </summary>
        /// <param name="name">Исходный код раздела.</param>
        public static string NormalizeSection(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return name.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        }

        /// <summary>
        /// Разбирает строку фильтра разделов и добавляет ошибки при неверном формате.
        /// </summary>
        /// <param name="sectionFilter">Строка фильтра (коды через запятую).</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        public static IList<string> ParseFilterTokens(string sectionFilter, ValidationResult result)
        {
            string value = sectionFilter?.Trim() ?? "";
            if (value.Length == 0)
                return Array.Empty<string>();

            string[] parts = value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => part.Length > 0)
                .ToArray();

            if (parts.Length == 0)
            {
                result.AddError("Поле «Раздел»: укажите код раздела (например, АК-1) или несколько через запятую.");
                return Array.Empty<string>();
            }

            var tokens = new List<string>();
            foreach (string part in parts)
            {
                if (!SectionTokenPattern.IsMatch(part))
                {
                    result.AddError(
                        $"Поле «Раздел»: недопустимое значение «{part}». " +
                        "Допустимы буквы, цифры и дефис (пример: АК-1, ЭМ-5).");
                    continue;
                }

                tokens.Add(part);
            }

            return tokens;
        }

        /// <summary>
        /// Собирает уникальные разделы из столбца Excel с учётом обрезки строк.
        /// </summary>
        /// <param name="data">Данные Excel в столбцовом виде.</param>
        /// <param name="cutTop">Число строк обрезки сверху.</param>
        /// <param name="cutBottom">Число строк обрезки снизу.</param>
        public static IDictionary<string, string> CollectDocumentSections(
            List<List<string>> data,
            int cutTop,
            int cutBottom)
        {
            var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (data == null || data.Count <= ExcelColSection || data[0] == null)
                return sections;

            int start = 1 + cutTop;
            int endExclusive = data[0].Count - cutBottom;
            if (start < 1)
                start = 1;
            if (endExclusive < start)
                endExclusive = start;

            for (int rowIndex = start; rowIndex < endExclusive; rowIndex++)
            {
                string sectionValue = GetCell(data, ExcelColSection, rowIndex);
                if (string.IsNullOrWhiteSpace(sectionValue))
                    continue;

                string normalized = NormalizeSection(sectionValue);
                if (normalized.Length == 0)
                    continue;

                if (!sections.ContainsKey(normalized))
                    sections[normalized] = sectionValue.Trim();
            }

            return sections;
        }

        /// <summary>
        /// Безопасно читает значение ячейки из столбцового представления Excel.
        /// </summary>
        /// <param name="data">Данные Excel в столбцовом виде.</param>
        /// <param name="columnIndex">Индекс столбца.</param>
        /// <param name="rowIndex">Индекс строки.</param>
        private static string GetCell(List<List<string>> data, int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || columnIndex >= data.Count)
                return string.Empty;

            List<string> column = data[columnIndex];
            if (column == null || rowIndex < 0 || rowIndex >= column.Count)
                return string.Empty;

            return column[rowIndex] ?? string.Empty;
        }
    }
}
