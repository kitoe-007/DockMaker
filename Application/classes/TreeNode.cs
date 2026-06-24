using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Autoaccept_electrical_app
{
    /// <summary>
    /// Узел дерева шаблонов или файлового хранилища.
    /// </summary>
    public class TreeNode : INotifyPropertyChanged
    {
        private bool _isView = true;
        private bool _isExpanded;

        /// <summary>Отображаемое имя узла.</summary>
        public string Name { get; set; }

        /// <summary>true — папка, false — файл или шаблон.</summary>
        public bool IsFolder { get; set; }

        /// <summary>Ключ шаблона («OJR», «ZHV») для листьев дерева шаблонов.</summary>
        public string TemplateKey { get; set; }

        /// <summary>Полный путь к файлу или папке в хранилище.</summary>
        public string FullPath { get; set; }

        /// <summary>Дочерние узлы.</summary>
        public ObservableCollection<TreeNode> Children { get; set; }

        /// <summary>Текст подсказки при наведении на шаблон.</summary>
        public string ToolTipText { get; set; }

        /// <summary>Развёрнут ли узел в TreeView.</summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value; 
                OnPropertyChanged();
            }
        }

        /// <summary>Видим ли узел при фильтрации поиском.</summary>
        public bool IsView
        {
            get => _isView;
            set
            {
                if (_isView == value) return;
                _isView = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Создаёт пустой узел с пустой коллекцией потомков.
        /// </summary>
        public TreeNode()
        {
            Children = new ObservableCollection<TreeNode>();
            IsView = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Уведомляет привязки об изменении свойства.
        /// </summary>
        /// <param name="propertyName">Имя изменившегося свойства.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
