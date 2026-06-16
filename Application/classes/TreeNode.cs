using System.Collections.ObjectModel;

namespace Autoaccept_electrical_app
{
    public class TreeNode
    {
        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public bool IsExpanded { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; }

        public TreeNode()
        {
            Children = new ObservableCollection<TreeNode>();
        }
    }
}
