using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace ProcessorBoostModeManager
{
    
    public partial class ProcessSelectionWindow : Window
    {
        public static MainWindow? MainWindowInstance { get; set; }
        public ObservableCollection<ProcessesModel> ProcessesInUI { get; set; } = new();

        public ProcessSelectionWindow()
        {
            InitializeComponent();
            App.ProcessSelectionInstance = this;

            DataContext = this;
            LoadProcesses();
        }

        // Load current running processes and display them in the ProcessSelectionWindow List
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
                    ProcessesInUI.Add(new ProcessesModel
                    {
                        Name = process.ProcessName,
                        Location = selectedProcessLocation,
                        Icon = ExtractIcon(selectedProcessLocation)
                    });
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error Loading selected process into list. {e.Message}");
                }
            }
        }

        // Add program from ProcessSelectionWindow List to the Database.json File
        private void ProcessesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProcessesListBox.SelectedItem is not ProcessesModel selectedProcess)
                return;

            try
            {
                var program = new ProgramModel
                {
                    Name = selectedProcess.Name,
                    Location = selectedProcess.Location,
                    BoostMode = "Disabled"
                };
                JsonService.AddToFile(program);
            }
            catch
            {
                MessageBox.Show("Couldn't add Windows protected process!");
            }
            Close();
        }

        public static BitmapSource? ExtractIcon(string programLocation)
        {
            try
            {
                using var extractedIcon = System.Drawing.Icon.ExtractAssociatedIcon(programLocation);
                if (extractedIcon != null)
                {
                    var icon = Imaging.CreateBitmapSourceFromHIcon(
                        extractedIcon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    return icon;
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Error getting image from process (system protected / path 'Unknown') + {programLocation}");
            }
            return null;
        }
    }

    public class ProcessesModel
    {
        public BitmapSource? Icon { get; set; } = null;
        public required string Name { get; set; }
        public required string Location { get; set; }
    }
}