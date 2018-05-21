using System.Windows;
using SpectrumAnalyzer.ViewModels;
using System.Windows.Controls;

namespace SpectrumAnalyzer.Views
{
    public partial class MainWindow : Window
    {
        private ViewModel _dataContext;

        public MainWindow()
        {
            InitializeComponent();
            _dataContext = DataContext as ViewModel;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.TransitionCheckBoxChanged(sender, e);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _dataContext.FileSelectionChanged(sender, e);
        }
    }
}

