using ProcessorBoostModeManager;
using System.Diagnostics;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace ProcessorBoostModeManager
{
    public partial class ProcessSelectionWindow : Window
    {
        public static MainWindow? MainWindowInstance { get; set; }
        public ProcessSelectionWindow()
        {
            InitializeComponent();
            App.ProcessSelectionInstance = this;
            LoadProcesses();
        }

        // Load current running processes and display them in the ProcessSelectionWindow List
        private void LoadProcesses()
        {
            ProcessesListBox.Items.Clear();

            var processes = Process.GetProcesses()
                .Select(p => p.ProcessName)
                .Distinct()
                .Order()
                .ToList();

            ProcessesListBox.ItemsSource = processes;
        }

        // Add program from ProcessSelectionWindow List to the Database.json File
        private void ProcessesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string selectedProcessName = ProcessesListBox.SelectedItem.ToString() ?? "Unknown Name";
            var selectedProcess = Process.GetProcessesByName(selectedProcessName).FirstOrDefault();
            if (selectedProcess == null) return;

            try
            {
                string selectedProcessLocation = selectedProcess.MainModule?.FileName ?? "Unknown Location";
                var program = new ProgramModel
                {
                    Name = selectedProcessName,
                    Location = selectedProcessLocation,
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
    }
}