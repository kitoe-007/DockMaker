using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoaccept_electrical_app.classes
{
    /// <summary>
    /// Диалоги выбора и сохранения файлов.
    /// </summary>
    internal class Dialogs
    {
        /// <summary>Путь к файлу, выбранному или указанному в последнем диалоге.</summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Открывает диалог выбора Excel-файла.
        /// </summary>
        /// <returns>true, если пользователь выбрал файл.</returns>
        public bool OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Открывает диалог сохранения DOCX-документа.
        /// </summary>
        /// <returns>true, если пользователь указал путь сохранения.</returns>
        public bool SaveFileDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Word documents (*.docx)|*.docx|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = "docx";
            if (saveFileDialog.ShowDialog() == true)
            {
                FilePath = saveFileDialog.FileName;
                return true;
            }
            return false;
        }
    }
}
