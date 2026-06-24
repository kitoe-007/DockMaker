using Autoaccept_electrical_app;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Autoaccept_electrical_app.classes
{
    public class StorageTreeServiceClass
    {
        public string DataRootPath => ResolveProjectPathClass.ResolveProjectPath("data");

        public bool IsPathInDataStorage(string fullPath)
        {
            return !string.IsNullOrEmpty(fullPath) && IsWithinDataRoot(fullPath);
        }

        public void EnsureDataRootExists()
        {
            Directory.CreateDirectory(DataRootPath);
        }

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

        public string GetTargetDirectory(TreeNode selectedNode)
        {
            if (selectedNode == null || string.IsNullOrEmpty(selectedNode.FullPath))
                return DataRootPath;

            if (selectedNode.IsFolder)
                return selectedNode.FullPath;

            return Path.GetDirectoryName(selectedNode.FullPath);
        }

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

        private static bool IsDataSourceExtension(string extension)
        {
            return extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".xls", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetRelativeDisplayPath(string root, string fullPath)
        {
            string relative = fullPath.Substring(root.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.IsNullOrEmpty(relative) ? Path.GetFileName(fullPath) : relative;
        }

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

        private bool IsWithinDataRoot(string fullPath)
        {
            string root = Path.GetFullPath(DataRootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string path = Path.GetFullPath(fullPath);
            return path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || string.Equals(path, root, StringComparison.OrdinalIgnoreCase);
        }

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

        private static bool ShouldShowFile(FileInfo file)
        {
            return !ShouldHideFileName(file.Name);
        }

        private static bool ShouldHideFileName(string fileName)
        {
            return string.Equals(fileName, ".gitkeep", StringComparison.OrdinalIgnoreCase);
        }

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
