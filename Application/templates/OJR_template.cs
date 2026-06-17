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
            dg.Columns[0].Header = "№/пп";
            dg.Columns[1].Header = "Дата выполнения работ";
            dg.Columns[2].Header = "Наименование работ, выполняемых в процессе строительства,\n реконструкции, капитального ремонта объекта капитального строительства";
            dg.Columns[3].Header = "Должность, фамилия, инициалы, подпись уполномоченного\nпредставителя лица, осуществляющего строительство";
        }

        private void set_data(DataGrid dg, List<List<string>> data, int cut_top, int cut_bottom)
        {
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

        private void clear_table(DataGrid dg)
        {
            dg.Columns.Clear();
            dg.ItemsSource = null;
        }

        public void create_table(DataGrid dg, List<List<string>> data)
        {
            clear_table(dg);

            if (data == null || data.Count < 5 || data[0] == null || data[4] == null || data[0].Count < 2)
                return;

            var table = new DataTable();
            table.Columns.Add("column_0", typeof(string));
            table.Columns.Add("column_1", typeof(string));
            table.Columns.Add("column_2", typeof(string));
            table.Columns.Add("column_3", typeof(string));

            for (int i = 1; i < data[0].Count; i++)
            {
                var row = table.NewRow();
                row[0] = i.ToString();
                row[1] = i < data[0].Count ? (data[0][i] ?? "") : "";
                row[2] = i < data[4].Count ? (data[4][i] ?? "") : "";
                row[3] = "";
                table.Rows.Add(row);
            }

            dg.AutoGenerateColumns = true;
            dg.ItemsSource = table.DefaultView;
            set_headers(dg);
        }
    }
}
