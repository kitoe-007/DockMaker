using System;
using System.Collections.ObjectModel;

namespace Autoaccept_electrical_app.classes
{
    public class TreeSearchFilterClass
    {
        public void TreeSearchFilter(ObservableCollection<TreeNode> folders, string searchName)
        {
            string query = (searchName ?? string.Empty).Trim();

            foreach (TreeNode node in folders)
            {
                if (node != null)
                    ApplyToNode(node, query);
            }
        }

        private bool ApplyToNode(TreeNode node, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                node.IsView = true;
                foreach (TreeNode child in node.Children)
                    ApplyToNode(child, query);
                return true;
            }

            bool childVisible = false;
            foreach (TreeNode child in node.Children)
            {
                if (ApplyToNode(child, query))
                    childVisible = true;
            }

            bool selfMatch = node.Name != null &&
                node.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

            bool visible = selfMatch || childVisible;
            node.IsView = visible;

            if (childVisible && node.IsFolder)
                node.IsExpanded = true;

            return visible;
        }
    }
}
