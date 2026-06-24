using Autoaccept_electrical_app.auxiliary_modules;
using Autoaccept_electrical_app.classes;
using Autoaccept_electrical_app.classes.validation;
using Autoaccept_electrical_app.templates;
using Autoaccept_electrical_app.windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Autoaccept_electrical_app
{
  /// <summary>
  /// Главное окно: выбор шаблона, параметры, предпросмотр таблицы и сохранение DOCX.
  /// </summary>
  public partial class MainWindow : Window
  {
    /// <summary>Дерево доступных шаблонов документов.</summary>
    public ObservableCollection<TreeNode> Folders { get; set; }

    /// <summary>Excel-файлы из хранилища для выбора источника данных.</summary>
    public ObservableCollection<DataSourceFileItem> DataSourceFiles { get; set; }

    private FileClass _current_file = new FileClass();
    private readonly TreeSearchFilterClass _treeSearchFilter = new TreeSearchFilterClass();
    private readonly StorageTreeServiceClass _storageTreeService = new StorageTreeServiceClass();

    // Ключ выбранного шаблона: "OJR" или "ZHV"; null — шаблон не выбран
    private string _currentTemplateKey;

    /// <summary>
    /// Инициализирует интерфейс, дерево шаблонов и список источников данных.
    /// </summary>
    public MainWindow()
    {
      InitializeComponent();
      Folders = BuildTemplateTree(); // создание дерева шаблонов
      DataSourceFiles = new ObservableCollection<DataSourceFileItem>();
      DataContext = this;
      ReloadDataSourceFiles();
      Activated += (_, __) => ReloadDataSourceFiles();
    }

    /// <summary>
    /// Обновляет список Excel-файлов в ComboBox, сохраняя текущий выбор.
    /// </summary>
    private void ReloadDataSourceFiles()
    {
      if (DataSourceFileComboBox == null)
        return;

      string selectedPath = DataSourceFileComboBox.SelectedValue as string;

      DataSourceFiles.Clear();
      foreach (DataSourceFileItem item in _storageTreeService.LoadDataSourceFiles())
        DataSourceFiles.Add(item);

      if (!string.IsNullOrEmpty(selectedPath)
          && DataSourceFiles.Any(file => file.FullPath == selectedPath))
      {
        DataSourceFileComboBox.SelectedValue = selectedPath;
      }
    }

    /// <summary>
    /// Создаёт корневые узлы дерева шаблонов (ОЖР и ЖВК) с подсказками.
    /// </summary>
    private static ObservableCollection<TreeNode> BuildTemplateTree()
    {
      return new ObservableCollection<TreeNode>
      {
        new TreeNode
        {
          Name = "ОЖР",
          IsFolder = false,
          TemplateKey = "OJR",
          ToolTipText =
            "Общий журнал работ — раздел 3.\n\n" +
            "Формирует таблицу сведений о выполнении работ в процессе строительства из Excel-файла: " +
            "даты выполнения, наименования работ с группировкой по разделам.\n\n" +
            "Настройки: фильтр по разделу (например, АК-1, ЭМ-5), обрезка лишних строк сверху и снизу, " +
            "подстановка должности и ФИО уполномоченного представителя.\n\n" +
            "Источник данных — Excel из хранилища «Данные». Результат можно просмотреть в таблице и сохранить в DOCX."
        },
        new TreeNode
        {
          Name = "ЖВК",
          IsFolder = false,
          TemplateKey = "ZHV",
          ToolTipText =
            "Журнал входного учёта и контроля качества материалов (ЖВК).\n\n" +
            "Создаёт таблицу по материалам и конструкциям из Excel: дата доставки, наименование, " +
            "поставщик, сопроводительные документы, вид и результат контроля, подпись контролирующего лица.\n\n" +
            "Дополнительно заполняются шапка документа (объект, адрес, организация, даты начала и окончания) " +
            "и значения по умолчанию для строк: поставщик, вид контроля, результат, ФИО.\n\n" +
            "Источник данных — Excel из хранилища «Данные». Результат сохраняется в DOCX с оформленной шапкой."
        }
      };
    }

    /// <summary>
    /// Фильтрует дерево шаблонов по введённому тексту.
    /// </summary>
    /// <param name="sender">Поле поиска шаблонов.</param>
    /// <param name="e">Аргументы события изменения текста.</param>
    private void SearchTemplatesTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      _treeSearchFilter.TreeSearchFilter(Folders, SearchTemplatesTextBox.Text);
    }

    /// <summary>
    /// Синхронизирует текстовую шапку предпросмотра ЖВК с полями формы.
    /// </summary>
    private void UpdateZhvPreviewHeader()
    {
            UpdateZhvPreviewHeaderClass.Update(
                ZhvPreviewObjectName,
                ZhvPreviewObjectAddress,
                ZhvPreviewOrganization,
                ZhvPreviewStartedDate,
                ZhvPreviewFinishedDate,
                ObjectNameTextBox.Text,
                ObjectAddressTextBox.Text,
                OrganizationTextBox.Text,
                StartedDateTextBox.Text,
                FinishedDateTextBox.Text);
     }

    /// <summary>
    /// Переключает панель параметров и сбрасывает предпросмотр при смене шаблона.
    /// </summary>
    /// <param name="sender">Дерево шаблонов.</param>
    /// <param name="e">Аргументы события смены выбранного элемента.</param>
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

        /// <summary>
        /// Раскрывает или сворачивает папку в дереве шаблонов по клику.
        /// </summary>
        /// <param name="sender">Элемент дерева TreeViewItem.</param>
        /// <param name="e">Аргументы события нажатия кнопки мыши.</param>
        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var item = sender as TreeViewItem;
      if (item?.DataContext is TreeNode node && node.IsFolder && node.Children.Count > 0)
      {
        if (e.OriginalSource is DependencyObject source)
        {
          var clickedItem = GetTreeViewItemFromSource(source);
          if (clickedItem != null && clickedItem != item)
            return;
        }

        item.IsExpanded = !item.IsExpanded;
        item.IsSelected = true;
        e.Handled = true;
      }
    }

    /// <summary>
    /// Находит родительский TreeViewItem для элемента визуального дерева.
    /// </summary>
    /// <param name="source">Элемент визуального дерева.</param>
    private static TreeViewItem GetTreeViewItemFromSource(DependencyObject source)
    {
      while (source != null)
      {
        if (source is TreeViewItem treeViewItem)
          return treeViewItem;

        source = VisualTreeHelper.GetParent(source);
      }

      return null;
    }

        /// <summary>
        /// Читает Excel через Python, валидирует данные и строит таблицу предпросмотра.
        /// </summary>
        /// <param name="sender">Кнопка «Сгенерировать».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void GenerateDocButton_Click(object sender, RoutedEventArgs e)
        {
            TemplateFormFields formFields = CollectFormFields();
            if (!ValidationDialog.ShowIfInvalid(GenerateDocumentValidator.Validate(formFields)))
                return;

            DocDataGrid.ItemsSource = null;
            
            int cut_top = 0;
            int cut_bottom = 0;

            int.TryParse(Numeric_up_down_cut_top.Text?.Trim(), out cut_top);
            int.TryParse(Numeric_up_down_cut_bottom.Text?.Trim(), out cut_bottom);

            if (cut_top < 0) cut_top = 0;
            if (cut_bottom < 0) cut_bottom = 0;
            string pythonExe = ResolveProjectPathClass.ResolveProjectPath("File_extractors", ".venv", "Scripts", "python.exe");
            string extractorPy = ResolveProjectPathClass.ResolveProjectPath("File_extractors", "XLSX_extractor.py");
            string xlsx = formFields.DataSourcePath;
            string json = launch_and_data_extraction.launch_script(pythonExe, extractorPy, xlsx);
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "Ошибки")
            {
                MessageBox.Show("Не удалось получить данные из Excel. Проверьте путь к Python/.venv и файлу Excel.xlsx.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _current_file.json_data = Json_to_array_convertor.ConvertJsonToStringArray(json);
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

            if (!ValidationDialog.ShowIfInvalid(
                    GenerateDocumentValidator.ValidateAgainstDocument(formFields, _current_file.json_data, cut_top, cut_bottom)))
                return;

            _current_file.template_key = _currentTemplateKey;
            string sectionFilter = formFields.SectionFilter ?? "";

            try
            {
                if (_currentTemplateKey == "OJR")
                {
                    string positionFio = formFields.Fio ?? "";
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
                        formFields.Supplier,
                        formFields.ControlType,
                        formFields.ControlResult,
                        formFields.Fio);
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

        /// <summary>
        /// Выбирает Excel-файл и добавляет его в хранилище при необходимости.
        /// </summary>
        /// <param name="sender">Кнопка выбора файла.</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void ChooseFileSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs();
            if (!dialog.OpenFileDialog())
                return;

            string filePath = dialog.FilePath;

            if (_storageTreeService.IsPathInDataStorage(filePath))
            {
                ReloadDataSourceFiles();
                DataSourceFileComboBox.SelectedValue = filePath;
                return;
            }

            if (!_storageTreeService.TryAddFile(_storageTreeService.DataRootPath, filePath, out string error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string addedFilePath = Path.Combine(_storageTreeService.DataRootPath, Path.GetFileName(filePath));
            ReloadDataSourceFiles();
            DataSourceFileComboBox.SelectedValue = addedFilePath;
        }


        /// <summary>Увеличивает значение «Обрезать сверху» на 1.</summary>
        /// <param name="sender">Кнопка «+».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void Numeric_top_up_button_Click(object sender, RoutedEventArgs e)
        {
            Numeric_Change_Class.Numeric_change(Numeric_up_down_cut_top, 1);
        }

        /// <summary>Уменьшает значение «Обрезать сверху» на 1.</summary>
        /// <param name="sender">Кнопка «−».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void Numeric_top_down_button_Click(object sender, RoutedEventArgs e)
        {
            Numeric_Change_Class.Numeric_change(Numeric_up_down_cut_top, -1);
        }

        /// <summary>Увеличивает значение «Обрезать снизу» на 1.</summary>
        /// <param name="sender">Кнопка «+».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void Numeric_bottom_up_button_Click(object sender, RoutedEventArgs e)
        {
            Numeric_Change_Class.Numeric_change(Numeric_up_down_cut_bottom, 1);
        }

        /// <summary>Уменьшает значение «Обрезать снизу» на 1.</summary>
        /// <param name="sender">Кнопка «−».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void Numeric_bottom_down_button_Click(object sender, RoutedEventArgs e)
        {
            Numeric_Change_Class.Numeric_change(Numeric_up_down_cut_bottom, -1);
        }

        /// <summary>
        /// Собирает значения полей формы в объект для валидации и генерации.
        /// </summary>
        private TemplateFormFields CollectFormFields()
        {
            return new TemplateFormFields
            {
                TemplateKey = _currentTemplateKey,
                DataSourcePath = DataSourceFileComboBox.SelectedValue as string,
                CutTopText = Numeric_up_down_cut_top.Text?.Trim(),
                CutBottomText = Numeric_up_down_cut_bottom.Text?.Trim(),
                SectionFilter = SectionTextBox.Text?.Trim(),
                Fio = _currentTemplateKey == "ZHV"
                    ? InspectorFioTextBox.Text?.Trim()
                    : PositionFioTextBox.Text?.Trim(),
                PositionFio = PositionFioTextBox.Text?.Trim(),
                ObjectName = ObjectNameTextBox.Text?.Trim(),
                ObjectAddress = ObjectAddressTextBox.Text?.Trim(),
                Organization = OrganizationTextBox.Text?.Trim(),
                StartedDate = StartedDateTextBox.Text?.Trim(),
                FinishedDate = FinishedDateTextBox.Text?.Trim(),
                Supplier = SupplierTextBox.Text?.Trim(),
                ControlType = ControlTypeTextBox.Text?.Trim(),
                ControlResult = ControlResultTextBox.Text?.Trim(),
                InspectorFio = InspectorFioTextBox.Text?.Trim()
            };
        }

        /// <summary>
        /// Формирует служебные поля шапки DOCX (ключи с «_») для Python-сборщика.
        /// </summary>
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

        /// <summary>
        /// Сохраняет таблицу предпросмотра в DOCX через Python-сборщик.
        /// </summary>
        /// <param name="sender">Кнопка «Сохранить».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            TemplateFormFields formFields = CollectFormFields();
            if (!ValidationDialog.ShowIfInvalid(
                    SaveDocumentValidator.Validate(_current_file.table_data, _current_file.template_key, formFields)))
                return;

            Dialogs dialog_manager = new Dialogs();
            if (!dialog_manager.SaveFileDialog())
                return;

            string pythonExe = ResolveProjectPathClass.ResolveProjectPath("File_extractors", ".venv", "Scripts", "python.exe");
            string Docx_builder = ResolveProjectPathClass.ResolveProjectPath("File_builder", "DOCX_table_builder.py");
            string outputDocx = dialog_manager.FilePath;

            string jsonPath = Path.Combine(Path.GetTempPath(), $"aae_table_{Guid.NewGuid():N}.json");
            try
            {
                var converter = new DataTable_to_json();
                File.WriteAllText(
                    jsonPath,
                    converter.ConvertDataTableToJson(_current_file.table_data, DocDataGrid, BuildSaveMetadata()),
                    new System.Text.UTF8Encoding(false));

                string result = launch_and_data_extraction.launch_script(pythonExe, Docx_builder, jsonPath, outputDocx);
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

        /// <summary>
        /// Открывает окно управления хранилищем данных.
        /// </summary>
        /// <param name="sender">Кнопка перехода в хранилище.</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void MoveToStorageButton_Click(object sender, RoutedEventArgs e)
        {
            var storage_window = new StorageWindow();
            storage_window.Owner = this;
            storage_window.ShowDialog();
            ReloadDataSourceFiles();
        }
    }
}
