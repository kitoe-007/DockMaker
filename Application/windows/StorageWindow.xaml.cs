using Autoaccept_electrical_app;
using Autoaccept_electrical_app.classes;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Autoaccept_electrical_app.windows
{
    /// <summary>
    /// Окно управления файловым хранилищем: папки, файлы, переименование и удаление.
    /// </summary>
    public partial class StorageWindow : Window
    {
        private readonly TreeSearchFilterClass _treeSearchFilter = new TreeSearchFilterClass();
        private readonly StorageTreeServiceClass _storageTreeService = new StorageTreeServiceClass();
        private TreeNode _selectedStorageNode;

        /// <summary>Дерево папок и файлов хранилища.</summary>
        public ObservableCollection<TreeNode> Folders { get; set; }

        /// <summary>
        /// Загружает дерево хранилища и привязывает его к интерфейсу.
        /// </summary>
        public StorageWindow()
        {
            InitializeComponent();
            Folders = new ObservableCollection<TreeNode>();
            DataContext = this;
            ReloadStorageTree();
        }

        /// <summary>
        /// Перечитывает дерево с диска и применяет текущий фильтр поиска.
        /// </summary>
        private void ReloadStorageTree()
        {
            Folders.Clear();
            foreach (TreeNode node in _storageTreeService.LoadRootNodes())
                Folders.Add(node);

            _treeSearchFilter.TreeSearchFilter(Folders, SearchTextBox.Text);
        }

        /// <summary>
        /// Фильтрует дерево хранилища по введённому тексту.
        /// </summary>
        /// <param name="sender">Поле поиска.</param>
        /// <param name="e">Аргументы события изменения текста.</param>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _treeSearchFilter.TreeSearchFilter(Folders, SearchTextBox.Text);
        }

        /// <summary>
        /// Запоминает выбранный узел дерева хранилища.
        /// </summary>
        /// <param name="sender">Дерево хранилища.</param>
        /// <param name="e">Аргументы события смены выбранного элемента.</param>
        private void StorageTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedStorageNode = e.NewValue as TreeNode;
        }

        /// <summary>
        /// Возвращает выбранный узел из поля или из TreeView.
        /// </summary>
        private TreeNode GetEffectiveSelectedNode()
        {
            return _selectedStorageNode ?? StorageTreeView.SelectedItem as TreeNode;
        }

        /// <summary>
        /// Создаёт новую папку в выбранном каталоге хранилища.
        /// </summary>
        /// <param name="sender">Кнопка «Добавить папку».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string folderName = Interaction.InputBox("Введите имя папки:", "Новая папка", "");
            if (string.IsNullOrWhiteSpace(folderName))
                return;

            string parentDirectory = _storageTreeService.GetTargetDirectory(GetEffectiveSelectedNode());

            if (!_storageTreeService.TryCreateFolder(parentDirectory, folderName, out string error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ReloadStorageTree();
        }

        /// <summary>
        /// Копирует выбранный файл в каталог хранилища.
        /// </summary>
        /// <param name="sender">Кнопка «Добавить файл».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetEffectiveSelectedNode();
            string destinationDirectory = _storageTreeService.GetTargetDirectory(selected);

            var dialog = new Dialogs();
            if (!dialog.OpenFileDialog())
                return;

            if (!_storageTreeService.TryAddFile(destinationDirectory, dialog.FilePath, out string error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ReloadStorageTree();
        }

        /// <summary>
        /// Возвращает выбранный узел или показывает предупреждение, если ничего не выбрано.
        /// </summary>
        private TreeNode GetSelectedNodeOrWarn()
        {
            var selected = GetEffectiveSelectedNode();
            if (selected == null || string.IsNullOrEmpty(selected.FullPath))
            {
                MessageBox.Show("Выберите файл или папку.", "Хранилище данных",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }

            return selected;
        }

        /// <summary>
        /// Переименовывает выбранный файл или папку.
        /// </summary>
        /// <param name="sender">Кнопка «Переименовать».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void RenameItemButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedNodeOrWarn();
            if (selected == null)
                return;

            string title = selected.IsFolder ? "Переименовать папку" : "Переименовать файл";
            string newName = Interaction.InputBox("Введите новое имя:", title, selected.Name);
            if (string.IsNullOrWhiteSpace(newName))
                return;

            if (!_storageTreeService.TryRename(selected, newName, out string error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ReloadStorageTree();
        }

        /// <summary>
        /// Удаляет выбранный файл или папку после подтверждения.
        /// </summary>
        /// <param name="sender">Кнопка «Удалить».</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedNodeOrWarn();
            if (selected == null)
                return;

            string itemType = selected.IsFolder ? "папку" : "файл";
            MessageBoxResult confirm = MessageBox.Show(
                $"Удалить {itemType} «{selected.Name}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            if (!_storageTreeService.TryDelete(selected, out string error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ReloadStorageTree();
        }

        /// <summary>
        /// Обрабатывает клик по папке: выбор и раскрытие/сворачивание.
        /// </summary>
        /// <param name="sender">Элемент дерева TreeViewItem.</param>
        /// <param name="e">Аргументы события нажатия кнопки мыши.</param>
        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;
            if (item?.DataContext is TreeNode node && node.IsFolder)
            {
                if (e.OriginalSource is DependencyObject source)
                {
                    var clickedItem = GetTreeViewItemFromSource(source);
                    if (clickedItem != null && clickedItem != item)
                        return;
                }

                item.IsSelected = true;
                _selectedStorageNode = node;

                if (node.Children.Count > 0)
                {
                    item.IsExpanded = !item.IsExpanded;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Открывает файл по двойному щелчку в системном приложении.
        /// </summary>
        /// <param name="sender">Элемент дерева TreeViewItem.</param>
        /// <param name="e">Аргументы события двойного щелчка.</param>
        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;
            if (item?.DataContext is TreeNode node && !node.IsFolder)
            {
                if (e.OriginalSource is DependencyObject source)
                {
                    var clickedItem = GetTreeViewItemFromSource(source);
                    if (clickedItem != null && clickedItem != item)
                        return;
                }

                if (!_storageTreeService.TryOpenFile(node, out string error))
                {
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

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
        /// Возвращает фокус на главное окно и закрывает окно хранилища.
        /// </summary>
        /// <param name="sender">Кнопка возврата к документам.</param>
        /// <param name="e">Аргументы события нажатия.</param>
        private void MoveToDocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                Owner.Activate();
                Owner.Focus();
            }
            Close();
        }
    }
}
