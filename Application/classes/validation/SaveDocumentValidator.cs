using System.Data;

namespace Autoaccept_electrical_app.classes.validation
{
    /// <summary>
    /// Валидация перед сохранением сгенерированного документа в DOCX.
    /// </summary>
    internal static class SaveDocumentValidator
    {
        /// <summary>
        /// Проверяет наличие таблицы, шаблона и обязательных полей для сохранения.
        /// </summary>
        /// <param name="tableData">Сгенерированная таблица предпросмотра.</param>
        /// <param name="templateKey">Ключ шаблона («OJR» или «ZHV»).</param>
        /// <param name="fields">Значения полей формы.</param>
        public static ValidationResult Validate(DataTable tableData, string templateKey, TemplateFormFields fields)
        {
            var result = new ValidationResult();

            if (tableData == null || tableData.Rows.Count == 0)
                result.AddError("Нет данных для сохранения. Сначала сгенерируйте документ.");

            if (string.IsNullOrWhiteSpace(templateKey))
                result.AddError("Не определён шаблон документа. Сгенерируйте документ заново.");

            if (!result.IsValid)
                return result;

            if (fields != null)
                CommonFieldsValidator.ValidateFio(fields, result);

            if (templateKey == "ZHV" && fields != null)
                ZhvFieldsValidator.Validate(fields, result);
            return result;
        }
    }
}
