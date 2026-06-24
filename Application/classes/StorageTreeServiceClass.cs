using Autoaccept_electrical_app;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Autoaccept_electrical_app.classes
{
    /// <summary>
    /// Файловые операции и построение дерева папки «data» (хранилище Excel и документов).
    /// </summary>
    public class StorageTreeServiceClass
    {
        /// <summary>Абсолютный путь к корню хранилища данных.</summary>
        public string DataRootPath => ResolveProjectPathClass.ResolveProjectPath("data");

        /// <summary>
        /// Проверяет, что путь находится внутри каталога хранилища.
        /// </summary>
        /// <param name="fullPath">Проверяемый абсолютный путь.</param>
        public bool IsPathInDataStorage(string fullPath)
        {
            return !string.IsNullOrEmpty(fullPath) && IsWithinDataRoot(fullPath);
        }

        /// <summary>
        /// Создаёт корневую папку хранилища, если её ещё нет.
        /// </summary>
        public void EnsureDataRootExists()
        {
            Directory.CreateDirectory(DataRootPath);
        }

        /// <summary>
        /// Загружает корневые узлы дерева хранилища (папки и файлы верхнего уровня).
        /// </summary>
        public IEnumerable<TreeNode> LoadRootNodes()
        {
            EnsureDataRootExists();

            foreach (string directoryPath in Directory.GetDirectories(DataRootPath)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                yield return CreateFolderNode(new DirectoryInfo(directoryPath));
            }

            foreach (string filePath in Directory.GetFiles(DataRootPath)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var file = new FileInfo(filePath);
                if (!ShouldShowFile(file))
                    continue;

                yield return CreateFileNode(file);
            }
        }

        /// <summary>
        /// Возвращает каталог для создания папки или копирования файла по выбранному узлу.
        /// </summary>
        /// <param name="selectedNode">Выбранный узел дерева или null.</param>
        public string GetTargetDirectory(TreeNode selectedNode)
        {
            if (selectedNode == null || string.IsNullOrEmpty(selectedNode.FullPath))
                return DataRootPath;

            if (selectedNode.IsFolder)
                return selectedNode.FullPath;

            return Path.GetDirectoryName(selectedNode.FullPath);
        }

        /// <summary>
        /// Перечисляет все Excel-файлы в хранилище для ComboBox источника данных.
        /// </summary>
        public IEnumerable<DataSourceFileItem> LoadDataSourceFiles()
        {
            EnsureDataRootExists();
            string root = Path.GetFullPath(DataRootPath);

            foreach (string filePath in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var fileInfo = new FileInfo(filePath);
                if (!ShouldShowFile(fileInfo) || !IsDataSourceExtension(fileInfo.Extension))
                    continue;

                yield return new DataSourceFileItem(filePath, GetRelativeDisplayPath(root, filePath));
            }
        }

        /// <summary>
        /// Проверяет, что расширение файла - поддерживаемый формат Excel.
        /// </summary>
        /// <param name="extension">Расширение файла с точкой.</param>
        private static bool IsDataSourceExtension(string extension)
        {
            return extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".xls", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Формирует относительный путь для отображения в списке файлов.
        /// </summary>
        /// <param name="root">Корневой каталог хранилища.</param>
        /// <param name="fullPath">Абсолютный путь к файлу.</param>
        private static string GetRelativeDisplayPath(string root, string fullPath)
        {
            string relative = fullPath.Substring(root.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.IsNullOrEmpty(relative) ? Path.GetFileName(fullPath) : relative;
        }

        /// <summary>
        /// Создаёт подпапку в указанном каталоге хранилища.
        /// </summary>
        /// <param name="parentDirectoryPath">Родительский каталог.</param>
        /// <param name="folderName">Имя новой папки.</param>
        /// <param name="error">Текст ошибки при неудаче.</param>
        public bool TryCreateFolder(string parentDirectoryPath, string folderName, out string error)
        {
            error = null;
            folderName = folderName?.Trim();

            if (string.IsNullOrWhiteSpace(folderName))
            {
                error = "Имя папки не может быть пустым.";
                return false;
            }

            if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                error = "Имя папки содержит недопустимые символы.";
                return false;
            }

            string newFolderPath = Path.Combine(parentDirectoryPath, folderName);
            if (Directory.Exists(newFolderPath))
            {
                error = "Папка с таким именем уже существует.";
                return false;
            }

            Directory.CreateDirectory(newFolderPath);
            return true;
        }

        /// <summary>
        /// Копирует файл в каталог хранилища.
        /// </summary>
        /// <param name="destinationDirectoryPath">Каталог назначения в хранилище.</param>
        /// <param name="sourceFilePath">Путь к исходному файлу.</param>
        /// <param name="error">Текст ошибки при неудаче.</param>
        public bool TryAddFile(string destinationDirectoryPath, string sourceFilePath, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                error = "Файл не найден.";
                return false;
            }

            string fileName = Path.GetFileName(sourceFilePath);
            string destinationPath = Path.Combine(destinationDirectoryPath, fileName);

            if (File.Exists(destinationPath))
            {
                error = "Файл с таким именем уже есть в выбранной папке.";
                return false;
            }

            File.Copy(sourceFilePath, destinationPath);
            return true;
        }

        /// <summary>
        /// Переименовывает файл или папку в хранилище.
        /// </summary>
        /// <param name="node">Узел переименовываемого элемента.</param>
        /// <param name="newName">Новое имя файла или папки.</param>
        /// <param name="error">Текст ошибки при неудаче.</param>
        public bool TryRename(TreeNode node, string newName, out string error)
        {
            error = null;
            newName = newName?.Trim();

            if (node == null || string.IsNullOrEmpty(node.FullPath))
            {
                error = "Элемент не выбран.";
                return false;
            }

            if (!IsWithinDataRoot(node.FullPath))
            {
                error = "Нельзя переименовать этот элемент.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                error = "Имя не может быть пустым.";
                return false;
            }

            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                error = "Имя содержит недопустимые символы.";
                return false;
            }

            if (string.Equals(node.Name, newName, StringComparison.OrdinalIgnoreCase))
                return true;

            string parentDirectory = node.IsFolder
                ? Directory.GetParent(node.FullPath)?.FullName
                : Path.GetDirectoryName(node.FullPath);

            if (string.IsNullOrEmpty(parentDirectory))
            {
                error = "Не удалось определить расположение элемента.";
                return false;
            }

            string newPath = Path.Combine(parentDirectory, newName);
            if (Directory.Exists(newPath) || File.Exists(newPath))
            {
                error = "Элемент с таким именем уже существует.";
                return false;
            }

            if (node.IsFolder)
            {
                if (!Directory.Exists(node.FullPath))
                {
                    error = "Папка не найдена.";
                    return false;
                }

                Directory.Move(node.FullPath, newPath);
            }
            else
            {
                if (!File.Exists(node.FullPath))
                {
                    error = "Файл не найден.";
                    return false;
                }

                if (ShouldHideFileName(Path.GetFileName(node.FullPath)))
                {
                    error = "Этот файл нельзя переименовать.";
                    return false;
                }

                File.Move(node.FullPath, newPath);
            }

            return true;
        }

        /// <summary>
        /// Открывает файл системным приложением по умолчанию.
        /// </summary>
        /// <param name="node">Узел файла для открытия.</param>
        /// <param name="error">Текст ошибки при неудаче.</param>
        public bool TryOpenFile(TreeNode node, out string error)
        {
            error = null;

            if (node == null || node.IsFolder || string.IsNullOrEmpty(node.FullPath))
            {
                error = "Выберите файл для открытия.";
                return false;
            }

            if (!IsWithinDataRoot(node.FullPath))
            {
                error = "Нельзя открыть этот файл.";
                return false;
            }

            if (!File.Exists(node.FullPath))
            {
                error = "Файл не найден.";
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo(node.FullPath)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                error = "Не удалось открыть файл:\n" + ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Удаляет файл или папку из хранилища.
        /// </summary>
        /// <param name="node">Узел удаляемого элемента.</param>
        /// <param name="error">Текст ошибки при неудаче.</param>
        public bool TryDelete(TreeNode node, out string error)
        {
            error = null;

            if (node == null || string.IsNullOrEmpty(node.FullPath))
            {
                error = "Элемент не выбран.";
                return false;
            }

            if (!IsWithinDataRoot(node.FullPath))
            {
                error = "Нельзя удалить этот элемент.";
                return false;
            }

            if (string.Equals(Path.GetFullPath(node.FullPath), Path.GetFullPath(DataRootPath), StringComparison.OrdinalIgnoreCase))
            {
                error = "Нельзя удалить корневую папку хранилища.";
                return false;
            }

            if (node.IsFolder)
            {
                if (!Directory.Exists(node.FullPath))
                {
                    error = "Папка не найдена.";
                    return false;
                }

                Directory.Delete(node.FullPath, recursive: true);
            }
            else
            {
                if (!File.Exists(node.FullPath))
                {
                    error = "Файл не найден.";
                    return false;
                }

                if (ShouldHideFileName(Path.GetFileName(node.FullPath)))
                {
                    error = "Этот файл нельзя удалить.";
                    return false;
                }

                File.Delete(node.FullPath);
            }

            return true;
        }

        /// <summary>
        /// Проверяет, что путь не выходит за пределы корня хранилища.
        /// </summary>
        /// <param name="fullPath">Проверяемый абсолютный путь.</param>
        private bool IsWithinDataRoot(string fullPath)
        {
            string root = Path.GetFullPath(DataRootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string path = Path.GetFullPath(fullPath);
            return path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || string.Equals(path, root, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Рекурсивно строит узел дерева для папки и её содержимого.
        /// </summary>
        /// <param name="directory">Папка на диске.</param>
        private static TreeNode CreateFolderNode(DirectoryInfo directory)
        {
            var node = new TreeNode
            {
                Name = directory.Name,
                IsFolder = true,
                FullPath = directory.FullName,
                IsView = true
            };

            foreach (DirectoryInfo childDirectory in directory.GetDirectories()
                         .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
            {
                node.Children.Add(CreateFolderNode(childDirectory));
            }

            foreach (FileInfo childFile in directory.GetFiles()
                         .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (!ShouldShowFile(childFile))
                    continue;

                node.Children.Add(CreateFileNode(childFile));
            }

            return node;
        }

        /// <summary>
        /// Определяет, показывать ли файл в дереве хранилища.
        /// </summary>
        /// <param name="file">Информация о файле.</param>
        private static bool ShouldShowFile(FileInfo file)
        {
            return !ShouldHideFileName(file.Name);
        }

        /// <summary>
        /// Скрывает служебные файлы (например, .gitkeep).
        /// </summary>
        /// <param name="fileName">Имя файла без пути.</param>
        private static bool ShouldHideFileName(string fileName)
        {
            return string.Equals(fileName, ".gitkeep", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Создаёт листовой узел дерева для файла.
        /// </summary>
        /// <param name="file">Информация о файле.</param>
        private static TreeNode CreateFileNode(FileInfo file)
        {
            return new TreeNode
            {
                Name = file.Name,
                IsFolder = false,
                FullPath = file.FullName,
                IsView = true
            };
        }
    }
}
