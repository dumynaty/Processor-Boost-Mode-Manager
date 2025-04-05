using ProcessorBoostModeManager.Common;
using ProcessorBoostModeManager.Models;
using ProcessorBoostModeManager.ViewModels;
using ProcessorBoostModeManager.Views;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ProcessorBoostModeManager
{
    [SupportedOSPlatform("windows")]
    public partial class ProcessSelectionWindow : Window
    {
        MainViewModel _mainViewModel;
        public List<ProgramViewModel> ProcessesInUI { get; set; } = new();

        public ProcessSelectionWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            DataContext = this;
            _mainViewModel = mainViewModel;
            LoadProcesses();
        }

        private void LoadProcesses()
        {
            var runningProcesses = Process.GetProcesses().Where(p =>
            {
                try
                {
                    string? filePath = p.MainModule?.FileName;
                    return !string.IsNullOrEmpty(filePath) &&
                           !filePath.StartsWith(@"C:\Windows\", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }).GroupBy(p => p.ProcessName).Select(g => g.First()).ToList();

            foreach (var process in runningProcesses)
            {
                try
                {
                    string selectedProcessLocation = process.MainModule?.FileName ?? "Unknown Location";
                    var program = new ProgramViewModel(new ProgramModel { Name = process.ProcessName, Location = selectedProcessLocation })
                    {
                        Icon = IconHandler.ExtractIcon(selectedProcessLocation)
                    };
                    ProcessesInUI.Add(program);
                }
                catch
                {
                    return;
                }
            }
        }
        private void ProcessesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProcessesListBox.SelectedItem is not ProgramViewModel selectedProcess)
                return;

            try
            {
                var program = new ProgramModel
                {
                    Name = selectedProcess.Name,
                    Location = selectedProcess.Location
                };
                _mainViewModel.DatabaseJSON.AddProgramToDatabase(program);
                _mainViewModel.StatusMessageLower = $"Program {program.Name} has been added to the Database!";
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't add Windows protected process! {ex.Message}");
            }
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _mainViewModel.UpdateProgram();
            
            // Handle Button Is Enable status
            //_mainViewModel.AddButton.IsEnabled = true;
        }
    }
}
