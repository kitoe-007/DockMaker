using System.IO;

namespace Autoaccept_electrical_app
{
    /// <summary>
    /// Элемент списка Excel-файлов из хранилища «Данные».
    /// </summary>
    public class DataSourceFileItem
    {
        /// <summary>Полный путь к файлу на диске.</summary>
        public string FullPath { get; }

        /// <summary>Имя файла без пути.</summary>
        public string FileName { get; }

        /// <summary>Отображаемое имя (относительный путь или имя файла).</summary>
        public string DisplayName { get; }

        /// <summary>
        /// Создаёт элемент списка источников данных.
        /// </summary>
        /// <param name="fullPath">Абсолютный путь к Excel-файлу.</param>
        /// <param name="displayName">Подпись в ComboBox; по умолчанию — имя файла.</param>
        public DataSourceFileItem(string fullPath, string displayName = null)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(fullPath);
            DisplayName = displayName ?? FileName;
        }
    }
}
