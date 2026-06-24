using System.Collections.Generic;
using System.Linq;

namespace Autoaccept_electrical_app.classes.validation
{
    /// <summary>
    /// Результат проверки полей формы: список ошибок и признак успеха.
    /// </summary>
    internal class ValidationResult
    {
        /// <summary>Список сообщений об ошибках.</summary>
        public IList<string> Errors { get; } = new List<string>();

        /// <summary>true, если ошибок нет.</summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>
        /// Добавляет сообщение об ошибке, если оно не пустое.
        /// </summary>
        /// <param name="message">Текст ошибки.</param>
        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Errors.Add(message);
        }

        /// <summary>
        /// Переносит ошибки из другого результата валидации.
        /// </summary>
        /// <param name="other">Другой результат валидации.</param>
        public void Merge(ValidationResult other)
        {
            if (other == null)
                return;

            foreach (string error in other.Errors)
                AddError(error);
        }

        /// <summary>
        /// Формирует нумерованный текст всех ошибок для MessageBox.
        /// </summary>
        public string GetMessage()
        {
            if (IsValid)
                return string.Empty;

            return string.Join("\n", Errors.Select((error, index) => $"{index + 1}. {error}"));
        }
    }
}
