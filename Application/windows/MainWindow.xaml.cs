using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Autoaccept_electrical_app
{
  public partial class MainWindow : Window
  {
    public ObservableCollection<TreeNode> Folders { get; set; }

    public MainWindow()
    {
      InitializeComponent();
      DataContext = this;
      Folders = BuildTemplateTree();
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
  }
}
