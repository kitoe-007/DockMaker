using Autoaccept_electrical_app;
using Autoaccept_electrical_app.classes;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Autoaccept_electrical_app.windows
{
    public partial class StorageWindow : Window
    {
        private readonly TreeSearchFilterClass _treeSearchFilter = new TreeSearchFilterClass();
        private readonly StorageTreeServiceClass _storageTreeService = new StorageTreeServiceClass();

        public ObservableCollection<TreeNode> Folders { get; set; }

        public StorageWindow()
        {
            InitializeComponent();
            Folders = new ObservableCollection<TreeNode>();
            DataContext = this;
            ReloadStorageTree();
        }

        private void ReloadStorageTree()
        {
            Folders.Clear();
            foreach (TreeNode node in _storageTreeService.LoadRootNodes())
                Folders.Add(node);

            _treeSearchFilter.TreeSearchFilter(Folders, SearchTextBox.Text);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _treeSearchFilter.TreeSearchFilter(Folders, SearchTextBox.Text);
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string folderName = Interaction.InputBox("Введите имя папки:", "Новая папка", "");
            if (string.IsNullOrWhiteSpace(folderName))
                return;

            var selected = StorageTreeView.SelectedItem as TreeNode;
            string parentDirectory = _storageTreeService.GetTargetDirectory(selected);

            if (!_storageTreeService.TryCreateFolder(parentDirectory, folderName, out string error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ReloadStorageTree();
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs();
            if (!dialog.OpenFileDialog())
                return;

            var selected = StorageTreeView.SelectedItem as TreeNode;
            string destinationDirectory = _storageTreeService.GetTargetDirectory(selected);

            if (!_storageTreeService.TryAddFile(destinationDirectory, dialog.FilePath, out string error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ReloadStorageTree();
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
