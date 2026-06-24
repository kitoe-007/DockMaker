using System.IO;

namespace Autoaccept_electrical_app
{
    public class DataSourceFileItem
    {
        public string FullPath { get; }
        public string FileName { get; }
        public string DisplayName { get; }

        public DataSourceFileItem(string fullPath, string displayName = null)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(fullPath);
            DisplayName = displayName ?? FileName;
        }
    }
}
