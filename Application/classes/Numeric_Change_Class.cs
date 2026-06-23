using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Autoaccept_electrical_app.classes
{
    internal class Numeric_Change_Class
    {
        public static void Numeric_change(TextBox numeric, int delta)
        {
            if (numeric.Text != "") {
                numeric.Text = numeric.Text.All(char.IsDigit) ? int.Parse(numeric.Text) + delta > -1 ? (int.Parse(numeric.Text) + delta).ToString() : "0" : "0";
            }
            else numeric.Text = "0";
            
        }
    }
}
