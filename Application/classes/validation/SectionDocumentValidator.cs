using System.Collections.Generic;
using System.Linq;

namespace Autoaccept_electrical_app.classes.validation
{
    /// <summary>
    /// Проверка наличия запрошенных разделов в данных Excel.
    /// </summary>
    internal static class SectionDocumentValidator
    {
        /// <summary>
        /// Сверяет фильтр разделов со списком разделов в документе с учётом обрезки строк.
        /// </summary>
        /// <param name="data">Данные Excel в столбцовом виде.</param>
        /// <param name="sectionFilter">Строка фильтра разделов.</param>
        /// <param name="cutTop">Число строк обрезки сверху.</param>
        /// <param name="cutBottom">Число строк обрезки снизу.</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        public static void Validate(
            List<List<string>> data,
            string sectionFilter,
            int cutTop,
            int cutBottom,
            ValidationResult result)
        {
            IList<string> requestedSections = SectionValidationHelper.ParseFilterTokens(sectionFilter, result);
            if (requestedSections.Count == 0 || !result.IsValid)
                return;

            if (data == null || data.Count == 0)
            {
                result.AddError("Не удалось проверить разделы: документ не содержит данных.");
                return;
            }

            IDictionary<string, string> documentSections = SectionValidationHelper.CollectDocumentSections(
                data,
                cutTop,
                cutBottom);

            if (documentSections.Count == 0)
            {
                result.AddError("В документе не найдено ни одного раздела. Проверьте файл Excel и параметры обрезки.");
                return;
            }

            foreach (string requested in requestedSections)
            {
                string normalized = SectionValidationHelper.NormalizeSection(requested);
                if (documentSections.ContainsKey(normalized))
                    continue;

                string available = string.Join(", ", documentSections.Values.OrderBy(name => name));
                result.AddError(
                    $"Раздел «{requested}» не найден в документе. " +
                    $"Доступные разделы: {available}.");
            }
        }
    }
}
