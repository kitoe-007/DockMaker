using Autoaccept_electrical_app;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Autoaccept_electrical_app.classes
{
    public class StorageTreeServiceClass
    {
        public string DataRootPath => ResolveProjectPathClass.ResolveProjectPath("data");

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
                yield return CreateFileNode(new FileInfo(filePath));
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
                node.Children.Add(CreateFileNode(childFile));
            }

            return node;
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
