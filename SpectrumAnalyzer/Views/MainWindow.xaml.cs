using System.Windows;
using SpectrumAnalyzer.ViewModels;

namespace SpectrumAnalyzer.Views
{
    public partial class MainWindow : Window
    {
        private ViewModel _dataContext;

        public MainWindow()
        {
            InitializeComponent();
            _dataContext = DataContext as ViewModel;
            Files.SelectionChanged += _dataContext.FilesAdded;
        }
    }
}

