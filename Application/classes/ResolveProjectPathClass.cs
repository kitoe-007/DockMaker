using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Autoaccept_electrical_app.classes
{
    internal class ResolveProjectPathClass
    {
        public static string ResolveProjectPath(params string[] relativeParts) // поиск путей к скриптам
        {
            var parts = new[] { AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".." }
              .Concat(relativeParts)
              .ToArray();
            return Path.GetFullPath(Path.Combine(parts)); // склеиваем пути и получаем абсолютный путь
        }
    }
}
