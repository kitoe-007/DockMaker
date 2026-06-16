using Autoaccept_electrical_app.auxiliary_modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Autoaccept_electrical_app.auxiliary_modules
{
    internal class launch_and_data_extraction
    {
        public
            string launch_script(string Script_path, params object[] args)
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

                p.StartInfo.UseShellExecute = false;       // перенаправления - false = Да
                p.StartInfo.RedirectStandardOutput = true; // перенаправить stdout
                p.StartInfo.RedirectStandardError = true;  // для ошибок
                p.StartInfo.CreateNoWindow = true;         // скрыть окно консоли

                p.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;

                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();

                p.WaitForExit(); // завершения процесса

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



