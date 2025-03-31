// Structure:
// Get list of currently running programs
// Check if the programs from database are running
// Update the list order and visuals

// NEED TO ADD
//
// ProcessSelectionWindow Icon -- COMPLETE --
// ProcessSelectionWindow Clean list -- COMPLETE --
// MainWindow RightClick property with ContextMenu -- COMPLETE --
// Refresh BoostMode after Removing program
// Logging file
// Database backup and recovery
// Startup Arguments
// File Extra Exit
// ComboBox options selection, Dark theme


using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using ComboBox = System.Windows.Controls.ComboBox;

namespace ProcessorBoostModeManager
{
    public partial class MainWindow : Window
    {
        private readonly RegistryStartupManager startupManager;
        public ObservableCollection<ProgramModel> ProgramsInUI { get; set; } = new();
        private readonly DispatcherTimer timer = new();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize this instance for other classes
            ProcessSelectionWindow.MainWindowInstance = this;
            App.MainWindowInstance = this;
            JsonService.MainWindowInstance = this;
            TextBoxHandling.MainWindowInstance = this;

            // Initialize checkboxes
            startupManager = new RegistryStartupManager();
            AutostartCheckBox.IsChecked = startupManager.IsAutostartEnabled();
            WindowsNotificationCheckBox.IsChecked = Properties.Settings.Default.WindowsNotificationEnabled;

            // Prepare the active GUID Value
            GUIDHandling.GetCurrentGUID();

            // Display Database programs on Window ListBox
            DataContext = this;
            UpdateUI();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                UpdateUI();
            };
            timer.Start();
        }

        // CheckBoxes Methods
        private void AutostartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            startupManager.RegisterStartup();
            TextBoxHandling.Upper("Application is registered to start with Windows!");
        }
        private void AutostartCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            startupManager.UnregisterStartup();
            TextBoxHandling.Upper("Application is unregistered from starting with Windows!");
        }
        private void WindowsNotificationCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.WindowsNotificationEnabled = true;
            Properties.Settings.Default.Save();
            TextBoxHandling.Upper("Application will notify changes with Windows Baloon Pop-up!");
        }
        private void WindowsNotificationCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.WindowsNotificationEnabled = false;
            Properties.Settings.Default.Save();
            TextBoxHandling.Upper("Application will not notify changes with Windows Baloon Pop-up!");
        }

        // Event actions for the buttons
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessSelectionWindow selection = new();
            selection.Show();
        }
        private void AddManuallyButton_Click(object sender, RoutedEventArgs e)
        {
            JsonService.ManualAdd();
        }
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListBox.SelectedItem is ProgramModel selectedProgram)
                JsonService.RemoveProcess(selectedProgram);
            else
                TextBoxHandling.Lower("Please select a program to remove!");
        }

        // Refresh UI
        private void UpdateUI()
        {
            bool differentList = false;
            var currentBoostModeHighestValue = Processes.highestBoostModeValue;
            var ProgramsInDatabase = Processes.GetUpdatedListOfPrograms();
            var newBoostModeHighestValue = Processes.highestBoostModeValue;

            ProgramsInDatabase = ProgramsInDatabase.OrderByDescending(p => p.IsRunning).ThenByDescending(p => p.HighestValue).ThenBy(p => p.Name).ToList();
            
            if (ProgramsInUI.Count == 0)
            {
                foreach (var program in ProgramsInDatabase)
                {
                    ProgramsInUI.Add(program);
                }
            }
            else
            {
                if (ProcessListBox.SelectedItem == null)
                {
                    TextBoxHandling.Lower($"Current mode: {GUIDHandling.GetWindowsProcessorBoostMode()}", true, true);
                }

                for (int i = 0; i < ProgramsInDatabase.Count; i++)
                {
                    if (ProgramsInUI[i].IsRunning != ProgramsInDatabase[i].IsRunning)
                    {
                        differentList = true;
                        break;
                    }
                }
                if (!differentList)
                    return;

                ProgramsInUI.Clear();
                foreach (var program in ProgramsInDatabase)
                {
                    ProgramsInUI.Add(program);
                }
            }
            
            if (currentBoostModeHighestValue != newBoostModeHighestValue)
            {
                try
                {
                    GUIDHandling.SetGUID();
                    if (WindowsNotificationCheckBox.IsChecked == true)
                    App.trayIcon.ShowBalloonTip(2500, "Status change:", $"Current mode set to: " +
                        $"{Processes.highestBoostModeValue}", ToolTipIcon.None);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("MW.UUI() - Access denied for changing processor boost mode. Check app permissions.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"MW.UUI() - Error accessing the registry: {ex.Message}");
                }
            }
        }

        // Manage changes in selections
        private void ProcessListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProcessListBox.SelectedItem is ProgramModel selectedProcess)
            {
                TextBoxHandling.Lower($"Process selected: {selectedProcess.Name}", true);
            }
        }
        private void ProcessListBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Run order: ComboBox check, Deselection.
            // Be able to open the ComboBox when the ListBox Item is selected
            var originalSource = e.OriginalSource as DependencyObject;
            while (originalSource != null)
            {
                if (originalSource is ComboBox)
                {
                    return;
                }
                originalSource = VisualTreeHelper.GetParent(originalSource);
            }

            // Deselect ListBox Item if the Left Button is pressed outside or on the same Item
            if (ProcessListBox.SelectedItem != null)
            {
                var clickedItem = ProcessListBox.ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
                if (clickedItem == null || clickedItem.IsSelected)
                {
                    ProcessListBox.SelectedItem = null;
                    e.Handled = true;
                }
            }
        }
        private void ProcessListBox_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Deselect ListBox Item if the Right Button is pressed outside an Item
            var clickedItem = ProcessListBox.ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
            if (clickedItem == null)
            {
                ProcessListBox.SelectedItem = null;
                e.Handled = true;
            }
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Make changes to the ComboBox only when it was clicked by mouse.
            // If not, the ComboBox will trigger infinite ChangedEvent loop
            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.IsMouseOver)
            {
                JsonService.SavePrograms(ProgramsInUI.ToList());
                ProgramsInUI.Clear();
                UpdateUI();
            }
        }
        private void ComboBoxProcess_DropDownClosed(object sender, EventArgs e)
        {
            if (System.Windows.Input.Mouse.DirectlyOver is ComboBox comboBox)
            {
                comboBox.IsDropDownOpen = true;
            }
        }

        private void ProcessListBox_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            // Don't open context menu if right clicked on the ComboBox
            var originalSource = e.OriginalSource as DependencyObject;
            while (originalSource != null)
            {
                if (originalSource is ComboBox || ProcessListBox.SelectedItem is not ProgramModel selectedProcess)
                {
                    e.Handled = true;
                    return;
                }
                originalSource = VisualTreeHelper.GetParent(originalSource);
            }
        }
        private void OpenFileLocationProperty_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListBox.SelectedItem is ProgramModel selectedProcess)
            {
                string? programPath = selectedProcess.Location;
                FileExplorer.ShowFileInExplorer(programPath);
                // Simpler but opens a new explorer.exe process every time it is called
                // System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{ programPath }\"");
                ProcessListBox.SelectedItem = null;
                e.Handled = true;
            }
        }


        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }
    }
}