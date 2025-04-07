using ProcessorBoostModeManager.ViewModels;
using ProcessorBoostModeManager.Views;
using System.Runtime.Versioning;
using System.Windows;

namespace ProcessorBoostModeManager
{
    [SupportedOSPlatform("windows")]
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

        private void ProcessesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewModel.AddProgramToDatabase() == true)
                Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _mainViewModel.AddButtonIsEnabled = true;
            _mainViewModel.UpdateProgram();
        }
    }
}
