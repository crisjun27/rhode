using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Rhode
{
    public class TestViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Test> _tests;
        public ObservableCollection<Test> Tests
        {
            get { return _tests; }
            set
            {
                if (_tests != value)
                {
                    _tests = value;
                    OnPropertyChanged(nameof(Tests));
                }
            }
        }

        public ICommand ExpandCommand { get; set; }
        public ICommand CollapseCommand { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand BackCommand { get; set; }

        private bool _anyTestChecked;
        public bool AnyTestChecked
        {
            get { return _anyTestChecked; }
            set
            {
                if (_anyTestChecked != value)
                {
                    _anyTestChecked = value;
                    OnPropertyChanged(nameof(AnyTestChecked));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TestViewModel()
        {
            Tests = new ObservableCollection<Test>();
            ExpandCommand = new RelayCommand(Expand, () => Tests.Any(t => !t.IsExpanded));
            CollapseCommand = new RelayCommand(Collapse, () => Tests.Any(t => t.IsExpanded));
            StartCommand = new RelayCommand(Start, () => AnyTestChecked);
            BackCommand = new RelayCommand(Back, () => AnyTestChecked);

            Tests.CollectionChanged += Tests_CollectionChanged;

            PopulateTests(); // Populate the Tests
        }

        private void Tests_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Test item in e.NewItems)
                {
                    item.PropertyChanged += Test_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Test item in e.OldItems)
                {
                    item.PropertyChanged -= Test_PropertyChanged;
                }
            }
        }

        private void Test_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Test.IsChecked))
            {
                AnyTestChecked = Tests.Any(t => t.IsChecked == true);
            }
        }

        private void PopulateTests()
        {
            AddTest(1);
            AddTest(2);
        }

        private void AddTest(int major)
        {
            var test = new Test { Major = major };
            for (int i = 1; i <= 4; i++)
            {
                test.Children.Add(new Test { Major = major, Minor = i, Parent = test });
            }
            Tests.Add(test);
        }

        private void Expand()
        {
            foreach (var test in Tests)
            {
                test.IsExpanded = true;
                ExpandChildren(test);
            }
        }
        private void ExpandChildren(Test parent)
        {
            foreach (var child in parent.Children)
            {
                child.IsExpanded = true;
                ExpandChildren(child);
            }
        }

        private void Collapse()
        {
            foreach (var test in Tests)
            {
                test.IsExpanded = false;
                CollapseChildren(test);
            }
        }

        private void CollapseChildren(Test parent)
        {
            foreach (var child in parent.Children)
            {
                child.IsExpanded = false;
                CollapseChildren(child);
            }
        }

        private void Start()
        {
            var checkedTests = GetCheckedTests(Tests);

            // convert the list of checked tests into a string
            var message = string.Join("\n", checkedTests.Select(t => $"Test {t.Major}.{t.Minor}"));

            // show a message box with all the checked tests
            MessageBox.Show(message, "Checked Tests");
        }

        private void Back()
        {
            UncheckAll(Tests);
        }

        private List<Test> GetCheckedTests(IEnumerable<Test> tests)
        {
            var checkedTests = new List<Test>();

            foreach (var test in tests)
            {
                if (test.IsChecked == true)
                    checkedTests.Add(test);

                if (test.Children != null && test.Children.Any())
                    checkedTests.AddRange(GetCheckedTests(test.Children));
            }

            return checkedTests;
        }

        private void UncheckAll(IEnumerable<Test> tests)
        {
            foreach (var test in tests)
            {
                test.IsChecked = false;

                if (test.Children != null && test.Children.Any())
                    UncheckAll(test.Children);
            }
        }
    }
}
