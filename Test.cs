using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Rhode
{
    public class Test : INotifyPropertyChanged
    {
        public Test()
        {
            Children = new ObservableCollection<Test>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Test item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Test item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsChecked))
            {
                NotifyPropertyChanged(nameof(IsChecked));
            }
        }

        public int Major { get; set; }
        public int Minor { get; set; }
        public Test Parent { get; set; }
        public ObservableCollection<Test> Children { get; set; }

        private bool? _isChecked = false;
        public bool? IsChecked
        {
            get
            {
                if (!Children.Any()) return _isChecked;
                if (Children.All(item => item.IsChecked.HasValue && item.IsChecked.Value)) return true;
                if (Children.All(item => item.IsChecked.HasValue && !item.IsChecked.Value)) return false;
                return null; // Indeterminate
            }
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;

                // Check/uncheck children
                foreach (var child in Children)
                {
                    child.IsChecked = value;
                }

                // Notify the parent item
                Parent?.NotifyPropertyChanged(nameof(IsChecked));
                NotifyPropertyChanged(nameof(IsChecked));
            }
        }

        public string DisplayName => $"Test {Major}.{Minor}";

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    NotifyPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
