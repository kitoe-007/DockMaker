using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace Autoaccept_electrical_app.classes
{
    /// <summary>
    /// Изменение числовых полей обрезки строк кнопками «+» и «−».
    /// </summary>
    internal class Numeric_Change_Class
    {
        /// <summary>
        /// Увеличивает или уменьшает значение в TextBox на заданную величину (не ниже 0).
        /// </summary>
        /// <param name="numeric">Поле ввода числа.</param>
        /// <param name="delta">Шаг изменения (+1 или −1).</param>
        public static void Numeric_change(TextBox numeric, int delta)
        {
            if (numeric.Text != "") {
                numeric.Text = numeric.Text.All(char.IsDigit) ? int.Parse(numeric.Text) + delta > -1 ? (int.Parse(numeric.Text) + delta).ToString() : "0" : "0";
            }
            else numeric.Text = "0";
            
        }
    }
}
