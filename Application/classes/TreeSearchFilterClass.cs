using System;
using System.Collections.ObjectModel;

namespace Autoaccept_electrical_app.classes
{
    /// <summary>
    /// Фильтрация дерева шаблонов или хранилища по текстовому запросу.
    /// </summary>
    public class TreeSearchFilterClass
    {
        /// <summary>
        /// Скрывает узлы, не совпадающие с запросом, и раскрывает родительские папки с совпадениями.
        /// </summary>
        /// <param name="folders">Корневая коллекция узлов дерева.</param>
        /// <param name="searchName">Строка поиска (без учёта регистра).</param>
        public void TreeSearchFilter(ObservableCollection<TreeNode> folders, string searchName)
        {
            string query = (searchName ?? string.Empty).Trim();

            foreach (TreeNode node in folders)
            {
                if (node != null)
                    ApplyToNode(node, query);
            }
        }

        /// <summary>
        /// Рекурсивно определяет видимость узла и его потомков.
        /// </summary>
        /// <param name="node">Текущий узел дерева.</param>
        /// <param name="query">Строка поиска (пустая — показать все узлы).</param>
        /// <returns>true, если узел или хотя бы один потомок видим.</returns>
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
