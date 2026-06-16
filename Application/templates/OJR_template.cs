using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Autoaccept_electrical_app.templates
{
    internal class OJR_template
    {
        public string Name { get; set; }
        
        private void set_headers(DataGrid dg)
        {
            dg.Columns[0] = "№/пп";
            dg.Columns[1] = "Дата выполнения работ";
            dg.Columns[2] = "Наименование работ, выполняемых в процессе строительства,\n реконструкции, капитального ремонта объекта капитального строительства";
            dg.Columns[3] = "Должность, фамилия, инициалы, подпись уполномоченного\nпредставителя лица, осуществляющего строительство";
        }

        private void set_data(DataGrid dg, List<List<string>> data, int cut_top, int cut_bottom) {
            for (int i = 1; i < data[0].Count; i++)
            {
                var row_ind = (DataRowView)dg.Items[0];
                row_ind.Row[i] = i;
                var row_date = (DataRowView)dg.Items[1];
                row_date.Row[i] = data[0][i];
                var row_data = (DataRowView)dg.Items[2];
                row_data.Row[i] = data[4][i];
            }
        }

        public void create_table(DataGrid dg, List<List<string>> data)
        {
            dg.Columns.Clear();
            dg.ItemsSource = null;
        }
    }
