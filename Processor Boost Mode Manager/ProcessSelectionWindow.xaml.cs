using ProcessorBoostModeManager.Common;
using ProcessorBoostModeManager.Models;
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
        MainWindow _mainWindow;
        public List<ProgramModel> ProcessesInUI { get; set; } = new();

        public ProcessSelectionWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            DataContext = this;
            _mainWindow = mainWindow;
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
                    ProcessesInUI.Add(new ProgramModel
                    {
                        Name = process.ProcessName,
                        Location = selectedProcessLocation,
                        Icon = IconHandler.ExtractIcon(selectedProcessLocation)
                    });
                }
                catch
                {
                    return;
                }
            }
        }
        private void ProcessesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProcessesListBox.SelectedItem is not ProgramModel selectedProcess)
                return;

            try
            {
                var program = new ProgramModel
                {
                    Name = selectedProcess.Name,
                    Location = selectedProcess.Location,
                    Icon = IconHandler.ExtractIcon(selectedProcess.Location)
                };
                Processes.AddProgramToDatabase(program);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't add Windows protected process! {ex.Message}");
            }
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _mainWindow.UpdateProgram();
            _mainWindow.AddButton.IsEnabled = true;
        }
    }
}