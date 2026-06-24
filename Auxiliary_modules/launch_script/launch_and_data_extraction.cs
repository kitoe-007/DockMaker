using Autoaccept_electrical_app.auxiliary_modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Autoaccept_electrical_app.auxiliary_modules
{
    /// <summary>
    /// Запуск Python-скриптов и чтение результата из stdout.
    /// </summary>
    internal class launch_and_data_extraction
    {
        /// <summary>
        /// Запускает внешний процесс (Python) с аргументами и возвращает stdout.
        /// </summary>
        /// <param name="Script_path">Путь к исполняемому файлу (python.exe).</param>
        /// <param name="args">Путь к скрипту и его аргументы.</param>
        /// <returns>Вывод скрипта или «Ошибки» при сбое.</returns>
        public
            static string launch_script(string Script_path, params object[] args)
            {
            try
            {
                string arguments = args != null && args.Length > 0
                ? string.Join(" ", args.Select(a => $"\"{a}\""))
                : "";

                if (arguments == "")
                {
                    return "";
                }

                Process p = new Process();

                p.StartInfo.FileName = Script_path;
                p.StartInfo.Arguments = arguments;

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;

                p.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;

                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();

                p.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"Ошибка скрипта: {error}");

                return output;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return "Ошибки";
            }
        }
    }
}


