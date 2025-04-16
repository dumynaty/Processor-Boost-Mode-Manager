using ProcessorBoostModeManager.ViewModels;
using System.Windows;
using System.Windows.Input;
namespace ProcessorBoostModeManager
{
    public partial class ProcessSelectionWindow : Window
    {
        private MainViewModel _mainViewModel;
        private ProcessSelectionViewModel _viewModel;

        public ProcessSelectionWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            _mainViewModel = mainViewModel;
            _viewModel = new ProcessSelectionViewModel(_mainViewModel);

            DataContext = _viewModel;
        }

        private void ProcessesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.AddProgramToDatabase() == true)
                Close();
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenFileLocation();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _mainViewModel.AddButtonIsEnabled = true;
        }
    }
}
