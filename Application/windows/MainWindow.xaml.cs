using Autoaccept_electrical_app.auxiliary_modules;
using Autoaccept_electrical_app.classes;
using Autoaccept_electrical_app.templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
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

    private FileClass _current_file = new FileClass();
    // Ключ выбранного шаблона: "OJR" или "ZHV"; null — шаблон не выбран
    private string _currentTemplateKey;

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
        new TreeNode { Name = "ОЖР", IsFolder = false, TemplateKey = "OJR" },
        new TreeNode { Name = "ЖВК", IsFolder = false, TemplateKey = "ZHV" }
      };
    }

    private static string ResolveProjectPath(params string[] relativeParts)
    {
      var parts = new[] { AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".." }
        .Concat(relativeParts)
        .ToArray();
      return Path.GetFullPath(Path.Combine(parts));
    }

    private void TemplatesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      var node = e.NewValue as TreeNode;
      bool isSelectedTemp = node != null && !node.IsFolder;
      bool isOjr = isSelectedTemp && node.TemplateKey == "OJR";
      bool isZhv = isSelectedTemp && node.TemplateKey == "ZHV";

      // Одна рабочая панель: общие поля (Excel, обрезка, раздел) + блоки OjrOnlyFields / ZhvOnlyFields
      TemplateWorkspacePanel.Visibility = isOjr || isZhv ? Visibility.Visible : Visibility.Collapsed;
      OjrOnlyFields.Visibility = isOjr ? Visibility.Visible : Visibility.Collapsed;
      ZhvOnlyFields.Visibility = isZhv ? Visibility.Visible : Visibility.Collapsed;
      OjrPreviewHeader.Visibility = isOjr ? Visibility.Visible : Visibility.Collapsed;
      ZhvPreviewHeader.Visibility = isZhv ? Visibility.Visible : Visibility.Collapsed;

      string newTemplateKey = isSelectedTemp ? node.TemplateKey : null;
      if (newTemplateKey != _currentTemplateKey)
      {
        // Сброс предпросмотра при смене шаблона, чтобы не показывать чужую таблицу
        DocDataGrid.ItemsSource = null;
        _current_file.table_data = null;
      }

      _currentTemplateKey = newTemplateKey;

      if (isZhv)
        UpdateZhvPreviewHeader();
    }

    // Текст шапки ЖВК в предпросмотре синхронизируется с полями параметров
    private void UpdateZhvPreviewHeader()
    {
      ZhvPreviewObjectName.Text = FormatLabeledField("Наименование объекта", ObjectNameTextBox.Text);
      ZhvPreviewObjectAddress.Text = FormatLabeledField("Адрес объекта", ObjectAddressTextBox.Text);
      ZhvPreviewOrganization.Text = FormatLabeledField("Наименование строительной организации", OrganizationTextBox.Text);
      ZhvPreviewStartedDate.Text = FormatLabeledField("Начат", StartedDateTextBox.Text);
      ZhvPreviewFinishedDate.Text = FormatLabeledField("Окончен", FinishedDateTextBox.Text);
    }

    private static string FormatLabeledField(string label, string value)
    {
      return string.IsNullOrWhiteSpace(value) ? "" : $"{label}  {value.Trim()}";
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
            if (string.IsNullOrEmpty(_currentTemplateKey))
            {
                MessageBox.Show("Выберите шаблон документа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DocDataGrid.ItemsSource = null;
            Json_to_array_convertor js_con = new Json_to_array_convertor();
            launch_and_data_extraction launch_script = new launch_and_data_extraction();
            
            int cut_top = 0;
            int cut_bottom = 0;

            if (!int.TryParse(Numeric_up_down_cut_top.Text, out cut_top))
                cut_top = 0;
            if (!int.TryParse(Numeric_up_down_cut_bottom.Text, out cut_bottom))
                cut_bottom = 0;

            if (cut_top < 0) cut_top = 0;
            if (cut_bottom < 0) cut_bottom = 0;
            string pythonExe = ResolveProjectPath("File_extractors", ".venv", "Scripts", "python.exe");
            string extractorPy = ResolveProjectPath("File_extractors", "XLSX_extractor.py");
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

            _current_file.json_data = js_con.ConvertJsonToStringArray(json);
            if (_current_file.json_data.Count == 0)
            {
                string preview = json.Trim();
                if (preview.Length > 800) preview = preview.Substring(0, 800) + "...";
                MessageBox.Show("Excel прочитан, но данные не найдены.\n\nПервые символы вывода Python:\n" + preview, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int rowCount = _current_file.json_data.Max(c => c?.Count ?? 0);
            if (rowCount <= 1)
            {
                MessageBox.Show("В Excel нет строк данных (есть только заголовок или пусто).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _current_file.template_key = _currentTemplateKey;
            string sectionFilter = SectionTextBox.Text?.Trim() ?? "";

            try
            {
                if (_currentTemplateKey == "OJR")
                {
                    string positionFio = PositionFioTextBox.Text?.Trim() ?? "";
                    var ojr_tem = new OJR_template();
                    ojr_tem.create_table(DocDataGrid, _current_file.json_data, cut_top, cut_bottom, _current_file, sectionFilter, positionFio);
                }
                else if (_currentTemplateKey == "ZHV")
                {
                    UpdateZhvPreviewHeader();
                    var zhv_tem = new ZHV_template();
                    zhv_tem.create_table(
                        DocDataGrid,
                        _current_file.json_data,
                        cut_top,
                        cut_bottom,
                        _current_file,
                        sectionFilter,
                        ControlTypeTextBox.Text,
                        ControlResultTextBox.Text,
                        InspectorFioTextBox.Text);
                }
                else
                {
                    MessageBox.Show("Выбран неподдерживаемый шаблон.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при создании таблицы:\n\n" + ex.ToString(),
                    "Критическая ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
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

        private void Numeric_change(TextBox numeric, int delta)
        {
            numeric.Text = Numeric_up_down_cut_top.Text.All(char.IsDigit) ? (int.Parse(numeric.Text) + delta).ToString() : "0";
        }
        private void Numeric_top_up_button_Click(object sender, RoutedEventArgs e)
        {
            bool isOnlyDigits = Numeric_up_down_cut_top.Text.All(char.IsDigit);
            if (isOnlyDigits) {
                int value = int.Parse(Numeric_up_down_cut_top.Text);
                Numeric_up_down_cut_top.Text = (value+1).ToString();
            }
            else
            {
                Numeric_up_down_cut_top.Text = "0";
            }
            
        }

        private void Numeric_top_down_button_Click(object sender, RoutedEventArgs e)
        {
            bool isOnlyDigits = Numeric_up_down_cut_top.Text.All(char.IsDigit);
            if (isOnlyDigits)
            {
                int value = int.Parse(Numeric_up_down_cut_top.Text);
                Numeric_up_down_cut_top.Text = (value - 1).ToString();
            }
            else
            {
                Numeric_up_down_cut_top.Text = "0";
            }
        }

        private void Numeric_bottom_up_button_Click(object sender, RoutedEventArgs e)
        {
            bool isOnlyDigits = Numeric_up_down_cut_bottom.Text.All(char.IsDigit);
            if (isOnlyDigits)
            {
                int value = int.Parse(Numeric_up_down_cut_bottom.Text);
                Numeric_up_down_cut_bottom.Text = (value + 1).ToString();
            }
            else
            {
                Numeric_up_down_cut_bottom.Text = "0";
            }
        }

        private void Numeric_bottom_down_button_Click(object sender, RoutedEventArgs e)
        {
            bool isOnlyDigits = Numeric_up_down_cut_bottom.Text.All(char.IsDigit);
            if (isOnlyDigits)
            {
                int value = int.Parse(Numeric_up_down_cut_bottom.Text);
                Numeric_up_down_cut_bottom.Text = (value - 1).ToString();
            }
            else
            {
                Numeric_up_down_cut_bottom.Text = "0";
            }
        }

        // Метаданные с префиксом «_» попадают в JSON и читаются DOCX_table_builder.py (не в таблицу)
        private Dictionary<string, string> BuildSaveMetadata()
        {
            var metadata = new Dictionary<string, string>
            {
                ["_template"] = _current_file.template_key ?? "OJR"
            };

            if (_current_file.template_key == "ZHV")
            {
                metadata["_object_name"] = ObjectNameTextBox.Text?.Trim() ?? "";
                metadata["_object_address"] = ObjectAddressTextBox.Text?.Trim() ?? "";
                metadata["_organization"] = OrganizationTextBox.Text?.Trim() ?? "";
                metadata["_started_date"] = StartedDateTextBox.Text?.Trim() ?? "";
                metadata["_finished_date"] = FinishedDateTextBox.Text?.Trim() ?? "";
            }

            return metadata;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_current_file.table_data == null || _current_file.table_data.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения. Сначала сгенерируйте документ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Dialogs dialog_manager = new Dialogs();
            if (!dialog_manager.SaveFileDialog())
                return;

            string pythonExe = ResolveProjectPath("File_extractors", ".venv", "Scripts", "python.exe");
            string Docx_builder = ResolveProjectPath("File_builder", "DOCX_table_builder.py");
            string outputDocx = dialog_manager.FilePath;

            string jsonPath = Path.Combine(Path.GetTempPath(), $"aae_table_{Guid.NewGuid():N}.json");
            try
            {
                var converter = new DataTable_to_json();
                File.WriteAllText(
                    jsonPath,
                    converter.ConvertDataTableToJson(_current_file.table_data, DocDataGrid, BuildSaveMetadata()),
                    new System.Text.UTF8Encoding(false));

                var launcher = new launch_and_data_extraction();
                string result = launcher.launch_script(pythonExe, Docx_builder, jsonPath, outputDocx);
                if (result == "Ошибки")
                    return;

                MessageBox.Show($"Документ сохранён:\n{outputDocx}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                if (File.Exists(jsonPath))
                    File.Delete(jsonPath);
            }
        }
    }
}
