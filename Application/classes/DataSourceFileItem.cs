using System.IO;

namespace Autoaccept_electrical_app
{
    public class DataSourceFileItem
    {
        public string FullPath { get; }
        public string FileName { get; }

        public DataSourceFileItem(string fullPath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(fullPath);
        }
    }
}
