using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Autoaccept_electrical_app
{
    public class TreeNode : INotifyPropertyChanged
    {
        private bool _isView = true;
        private bool _isExpanded;

        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public string TemplateKey { get; set; }
        public string FullPath { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; }

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

        public TreeNode()
        {
            Children = new ObservableCollection<TreeNode>();
            IsView = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
