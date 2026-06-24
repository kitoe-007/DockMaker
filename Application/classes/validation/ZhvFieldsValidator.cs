using System;
using System.Globalization;

namespace Autoaccept_electrical_app.classes.validation
{
    /// <summary>
    /// Проверка полей шапки и строк по умолчанию для шаблона ЖВК.
    /// </summary>
    internal static class ZhvFieldsValidator
    {
        private static readonly string[] DateFormats =
        {
            "dd.MM.yyyy",
            "d.M.yyyy",
            "dd.MM.yy",
            "d.M.yy"
        };

        /// <summary>
        /// Валидирует обязательные поля ЖВК и корректность дат.
        /// </summary>
        /// <param name="fields">Значения полей формы ЖВК.</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        public static void Validate(TemplateFormFields fields, ValidationResult result)
        {
            if (fields == null)
                return;

            RequireNotEmpty(fields.ObjectName, "Наименование объекта", result);
            RequireNotEmpty(fields.ObjectAddress, "Адрес объекта", result);
            RequireNotEmpty(fields.Organization, "Строительная организация", result);
            RequireNotEmpty(fields.Supplier, "Поставщик", result);
            RequireNotEmpty(fields.ControlType, "Вид контроля", result);
            RequireNotEmpty(fields.ControlResult, "Результат контроля", result);

            ValidateOptionalDate(fields.StartedDate, "Начат", out DateTime? startedDate, result);
            ValidateOptionalDate(fields.FinishedDate, "Окончен", out DateTime? finishedDate, result);

            if (startedDate.HasValue && finishedDate.HasValue && finishedDate.Value < startedDate.Value)
            {
                result.AddError("Дата «Окончен» не может быть раньше даты «Начат».");
            }
        }

        /// <summary>
        /// Добавляет ошибку, если строковое поле пустое.
        /// </summary>
        /// <param name="value">Проверяемое значение.</param>
        /// <param name="fieldLabel">Подпись поля для сообщения об ошибке.</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        private static void RequireNotEmpty(string value, string fieldLabel, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(value))
                result.AddError($"Поле «{fieldLabel}»: обязательно для заполнения.");
        }

        /// <summary>
        /// Проверяет необязательную дату в формате ДД.ММ.ГГГГ.
        /// </summary>
        /// <param name="value">Текст поля даты.</param>
        /// <param name="fieldLabel">Подпись поля для сообщения об ошибке.</param>
        /// <param name="parsedDate">Распознанная дата или null.</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        private static void ValidateOptionalDate(
            string value,
            string fieldLabel,
            out DateTime? parsedDate,
            ValidationResult result)
        {
            parsedDate = null;
            string text = value?.Trim() ?? "";

            if (text.Length == 0)
                return;

            if (DateTime.TryParseExact(
                    text,
                    DateFormats,
                    CultureInfo.GetCultureInfo("ru-RU"),
                    DateTimeStyles.None,
                    out DateTime exactDate))
            {
                parsedDate = exactDate;
                return;
            }

            if (DateTime.TryParse(text, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out DateTime looseDate))
            {
                parsedDate = looseDate;
                return;
            }

            result.AddError($"Поле «{fieldLabel}»: укажите дату в формате ДД.ММ.ГГГГ (например, 15.06.2025).");
        }
    }
}
