using System.Windows;

namespace Autoaccept_electrical_app.classes.validation
{
    /// <summary>
    /// Показ предупреждения при неуспешной валидации полей.
    /// </summary>
    internal static class ValidationDialog
    {
        /// <summary>
        /// Показывает MessageBox с ошибками, если валидация не пройдена.
        /// </summary>
        /// <param name="result">Результат валидации.</param>
        /// <param name="title">Заголовок окна сообщения.</param>
        /// <returns>true, если ошибок нет и можно продолжать действие.</returns>
        public static bool ShowIfInvalid(ValidationResult result, string title = "Проверка полей")
        {
            if (result == null || result.IsValid)
                return true;

            MessageBox.Show(result.GetMessage(), title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
    }
}
