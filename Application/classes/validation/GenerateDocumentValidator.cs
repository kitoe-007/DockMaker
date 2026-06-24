using System.Collections.Generic;

namespace Autoaccept_electrical_app.classes.validation
{
    /// <summary>
    /// Валидация полей перед генерацией документа из Excel.
    /// </summary>
    internal static class GenerateDocumentValidator
    {
        /// <summary>
        /// Проверяет форму перед запуском Python-экстрактора и построения таблицы.
        /// </summary>
        /// <param name="fields">Значения полей формы.</param>
        public static ValidationResult Validate(TemplateFormFields fields)
        {
            var result = new ValidationResult();

            CommonFieldsValidator.Validate(fields, result);

            if (fields == null || !result.IsValid)
                return result;

            switch (fields.TemplateKey)
            {
                case "ZHV":
                    ZhvFieldsValidator.Validate(fields, result);
                    break;
                case "OJR":
                    break;
                default:
                    if (!string.IsNullOrWhiteSpace(fields.TemplateKey))
                        result.AddError("Выбран неподдерживаемый шаблон документа.");
                    break;
            }

            return result;
        }

        /// <summary>
        /// Проверяет, что запрошенные разделы присутствуют в загруженном Excel.
        /// </summary>
        /// <param name="fields">Значения полей формы (фильтр раздела).</param>
        /// <param name="jsonData">Данные Excel в столбцовом виде.</param>
        /// <param name="cutTop">Число строк обрезки сверху.</param>
        /// <param name="cutBottom">Число строк обрезки снизу.</param>
        public static ValidationResult ValidateAgainstDocument(
            TemplateFormFields fields,
            List<List<string>> jsonData,
            int cutTop,
            int cutBottom)
        {
            var result = new ValidationResult();
            SectionDocumentValidator.Validate(jsonData, fields?.SectionFilter, cutTop, cutBottom, result);
            return result;
        }
    }
}
