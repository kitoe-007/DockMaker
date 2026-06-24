using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoaccept_electrical_app.classes
{
    /// <summary>
    /// Состояние текущего документа: сырые данные Excel и сформированная таблица.
    /// </summary>
    internal class FileClass
    {
        /// <summary>Данные Excel в виде столбцов строк (результат Python-экстрактора).</summary>
        public List<List<string>> json_data { get; set;  }

        /// <summary>Таблица предпросмотра и сохранения в DOCX.</summary>
        public DataTable table_data { get; set; }

        /// <summary>Ключ шаблона: «OJR» или «ZHV».</summary>
        public string template_key { get; set; }
    }
}
