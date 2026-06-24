using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Autoaccept_electrical_app.classes
{
    /// <summary>
    /// Построение абсолютных путей к файлам проекта относительно каталога приложения.
    /// </summary>
    internal class ResolveProjectPathClass
    {
        /// <summary>
        /// Собирает абсолютный путь к файлу или папке внутри репозитория.
        /// </summary>
        /// <param name="relativeParts">Части пути относительно корня проекта.</param>
        public static string ResolveProjectPath(params string[] relativeParts)
        {
            var parts = new[] { AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".." }
              .Concat(relativeParts)
              .ToArray();
            return Path.GetFullPath(Path.Combine(parts));
        }
    }
}
