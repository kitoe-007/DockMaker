using Autoaccept_electrical_app.auxiliary_modules;
using Autoaccept_electrical_app.classes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Autoaccept_electrical_app
{
  public partial class MainWindow : Window
  {
    public ObservableCollection<TreeNode> Folders { get; set; }
    public ObservableCollection<DataSourceFileItem> DataSourceFiles { get; set; }

    public MainWindow()
    {
      InitializeComponent();
      Folders = BuildTemplateTree();
      DataSourceFiles = new ObservableCollection<DataSourceFileItem>();
      DataContext = this;
    }

    private static ObservableCollection<TreeNode> BuildTemplateTree()
    {
      return new ObservableCollection<TreeNode>
      {
        new TreeNode
        {
          Name = "Договоры",
          IsFolder = true,
          IsExpanded = true,
          Children = new ObservableCollection<TreeNode>
          {
            new TreeNode { Name = "Договор поставки", IsFolder = false },
            new TreeNode { Name = "Договор подряда", IsFolder = false },
            new TreeNode { Name = "Доп. соглашение", IsFolder = false },
            new TreeNode { Name = "Спецификация", IsFolder = false }
          }
        },
        new TreeNode
        {
          Name = "Приказы",
          IsFolder = true,
          Children = new ObservableCollection<TreeNode>
          {
            new TreeNode { Name = "Приказ о приёме", IsFolder = false },
            new TreeNode { Name = "Приказ об увольнении", IsFolder = false }
          }
        },
        new TreeNode
        {
          Name = "Акты",
          IsFolder = true,
          Children = new ObservableCollection<TreeNode>
          {
            new TreeNode { Name = "Акт выполненных работ", IsFolder = false },
            new TreeNode { Name = "Акт приёма-передачи", IsFolder = false }
          }
        },
        new TreeNode
        {
          Name = "Служебные записки",
          IsFolder = true,
          Children = new ObservableCollection<TreeNode>
          {
            new TreeNode { Name = "Служебная записка", IsFolder = false }
          }
        },
        new TreeNode
        {
          Name = "Прочее",
          IsFolder = true,
          Children = new ObservableCollection<TreeNode>
          {
            new TreeNode { Name = "Шаблон без категории", IsFolder = false }
          }
        }
      };
    }

    private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var item = sender as TreeViewItem;
      if (item?.DataContext is TreeNode node && node.IsFolder && node.Children.Count > 0)
      {
        item.IsExpanded = !item.IsExpanded;
        item.IsSelected = true;
        e.Handled = true;
      }
    }

        private void GenerateDocButton_Click(object sender, RoutedEventArgs e)
        {
            DocDataGrid.ItemsSource = null;
            Json_to_array_convertor js_con = new Json_to_array_convertor();
            launch_and_data_extraction launch_script = new launch_and_data_extraction();

            string pythonExe = @"C:\Users\user\Desktop\работа\Приложения\Паша\git\autoaccept-electrical\File_extractors\.venv\Scripts\python.exe";
            string extractorPy = @"C:\Users\user\Desktop\работа\Приложения\Паша\git\autoaccept-electrical\File_extractors\XLSX_extractor.py";
            string xlsx = DataSourceFileComboBox.SelectedValue as string;
            if (xlsx == null) 
            {
                MessageBox.Show("Не выбран источник данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } 
            string json = launch_script.launch_script(pythonExe, extractorPy, xlsx);
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "Ошибки")
            {
                MessageBox.Show("Не удалось получить данные из Excel. Проверьте путь к Python/.venv и файлу Excel.xlsx.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<List<string>> columns = js_con.ConvertJsonToStringArray(json);
            if (columns.Count == 0)
            {
                string preview = json.Trim();
                if (preview.Length > 800) preview = preview.Substring(0, 800) + "...";
                MessageBox.Show("Excel прочитан, но данные не найдены.\n\nПервые символы вывода Python:\n" + preview, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int rowCount = columns.Max(c => c?.Count ?? 0);
            if (rowCount <= 1)
            {
                MessageBox.Show("В Excel нет строк данных (есть только заголовок или пусто).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var headers = columns.Select((col, i) =>
            {
                var h = (col != null && col.Count > 0) ? col[0] : null;
                return string.IsNullOrWhiteSpace(h) ? $"column_{i}" : h.Trim();
            }).ToList();

            var table = new DataTable();
            var used = new HashSet<string>();
            for (int c = 0; c < headers.Count; c++)
            {
                string name = headers[c];
                if (string.IsNullOrWhiteSpace(name)) name = $"column_{c}";
                string baseName = name;
                int k = 2;
                while (used.Contains(name))
                {
                    name = $"{baseName}_{k}";
                    k++;
                }
                used.Add(name);
                headers[c] = name;
                table.Columns.Add(name, typeof(string));
            }

            for (int r = 1; r < rowCount; r++)
            {
                var row = table.NewRow();
                for (int c = 0; c < columns.Count; c++)
                {
                    string value = (columns[c] != null && columns[c].Count > r) ? columns[c][r] : "";
                    row[headers[c]] = value ?? "";
                }
                table.Rows.Add(row);
            }

            DocDataGrid.ItemsSource = table.DefaultView;
        }

        private void ChooseFileSourceButton_Click(object sender, RoutedEventArgs e)
        {
            Dialogs dialog_manager = new Dialogs();
            if (!dialog_manager.OpenFileDialog())
                return;

            string filePath = dialog_manager.FilePath;
            if (!DataSourceFiles.Any(f => f.FullPath == filePath))
                DataSourceFiles.Add(new DataSourceFileItem(filePath));

            DataSourceFileComboBox.SelectedValue = filePath;
        }
    }
}
