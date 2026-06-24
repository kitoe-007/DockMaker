using System;
using System.Globalization;
using System.IO;
namespace Autoaccept_electrical_app.classes.validation
{
    /// <summary>
    /// Проверка общих полей формы для всех шаблонов (Excel, обрезка, ФИО, раздел).
    /// </summary>
    internal static class CommonFieldsValidator
    {
        /// <summary>
        /// Валидирует обязательные и форматные поля, общие для ОЖР и ЖВК.
        /// </summary>
        /// <param name="fields">Значения полей формы.</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        public static void Validate(TemplateFormFields fields, ValidationResult result)        {
            if (fields == null)
            {
                result.AddError("Не переданы данные формы.");
                return;
            }

            if (string.IsNullOrWhiteSpace(fields.TemplateKey))
                result.AddError("Выберите шаблон документа.");

            ValidateDataSource(fields.DataSourcePath, result);
            ValidateCutValue(fields.CutTopText, "Обрезать сверху", result);
            ValidateCutValue(fields.CutBottomText, "Обрезать снизу", result);
            ValidateFio(fields, result);
            ValidateSectionFilterFormat(fields.SectionFilter, result);
        }

        /// <summary>
        /// Проверяет заполненность поля ФИО ответственного лица.
        /// </summary>
        /// <param name="fields">Значения полей формы.</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        public static void ValidateFio(TemplateFormFields fields, ValidationResult result)
        {
            if (fields == null)
                return;

            if (string.IsNullOrWhiteSpace(fields.Fio))
                result.AddError("Поле «ФИО»: укажите фамилию с инициалами ответственного лица.");
        }

        /// <summary>
        /// Проверяет наличие и тип файла источника данных.
        /// </summary>
        /// <param name="path">Путь к Excel-файлу.</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        private static void ValidateDataSource(string path, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                result.AddError("Выберите источник данных (файл Excel).");
                return;
            }

            if (!File.Exists(path))
            {
                result.AddError("Файл источника данных не найден. Выберите другой файл или добавьте его в хранилище.");
                return;
            }

            string extension = Path.GetExtension(path);
            if (!extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                && !extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
            {
                result.AddError("Источник данных должен быть файлом Excel (.xlsx или .xls).");
            }
        }

        /// <summary>
        /// Проверяет, что значение обрезки — целое число ≥ 0.
        /// </summary>
        /// <param name="text">Текст поля обрезки.</param>
        /// <param name="fieldLabel">Подпись поля для сообщения об ошибке.</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        private static void ValidateCutValue(string text, string fieldLabel, ValidationResult result)
        {
            string value = text?.Trim() ?? "";

            if (value.Length == 0)
                return;

            if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
                || number < 0)
            {
                result.AddError($"Поле «{fieldLabel}»: укажите целое число не меньше 0.");
            }
        }

        /// <summary>
        /// Проверяет синтаксис фильтра разделов.
        /// </summary>
        /// <param name="sectionFilter">Строка фильтра разделов.</param>
        /// <param name="result">Объект для накопления ошибок.</param>
        private static void ValidateSectionFilterFormat(string sectionFilter, ValidationResult result)
        {
            SectionValidationHelper.ParseFilterTokens(sectionFilter, result);
        }
    }
}
