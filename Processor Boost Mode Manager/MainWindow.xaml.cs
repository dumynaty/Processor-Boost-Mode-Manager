using Processor_Boost_Mode_Manager;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using ComboBox = System.Windows.Controls.ComboBox;

namespace ProcessBoostModeManager
{
    public partial class MainWindow : Window
    {
        private readonly RegistryStartupManager startupManager;
        public ObservableCollection<ProgramModel> ProgramsInUI= new();
        private readonly DispatcherTimer timer = new();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize this instance for other classes
            ProcessSelectionWindow.MainWindowInstance = this;
            App.MainWindowInstance = this;
            JsonService.MainWindowInstance = this;
            TextBoxHandling.MainWindowInstance = this;

            // Initialize checkbox
            startupManager = new RegistryStartupManager();
            AutostartCheckBox.IsChecked = startupManager.IsAutostartEnabled();

            // Prepare the active GUID Value
            GUIDHandling.GetCurrentGUID();

            // Display Database programs on Window ListBox
            ProcessListBox.ItemsSource = ProgramsInUI;
            UpdateUI();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                UpdateUI();
            };
            timer.Start();
        }


        // CheckBoxes Methods
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            startupManager.RegisterStartup();
            TextBoxHandling.Upper("Application is registered to stat with Windows!");
        } 
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            startupManager.UnregisterStartup();
            TextBoxHandling.Upper("Application is unregistered from starting with Windows!");
        }

        // Event actions for the buttons
        public void AddButton_Click(object sender, RoutedEventArgs e)
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
        public void UpdateUI()
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
                    TextBoxHandling.Lower($"Current mode: {GUIDHandling.GetCurrentProcessorBoostMode()}", true, true);
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
            // If clicked on the ComboBox while a selection is made
            var originalSource = e.OriginalSource as DependencyObject;
            while (originalSource != null)
            {
                if (originalSource is ComboBox)
                {
                    return;
                }
                originalSource = VisualTreeHelper.GetParent(originalSource);
            }

            // Unselect program from the list if clicked twice
            if (ProcessListBox.SelectedItem != null)
            {
                var clickedItem = ProcessListBox.ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
                if (clickedItem != null && clickedItem.IsSelected)
                {
                    ProcessListBox.SelectedItem = null;
                    e.Handled = true;
                }
            }
        }

        public bool comboBoxIsSelectedByUser = false; 
        private void ProcessListBox_ComboBoxPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            comboBoxIsSelectedByUser = true;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxIsSelectedByUser)
            {
                JsonService.SavePrograms(ProgramsInUI.ToList());
                ProgramsInUI.Clear();
                UpdateUI();
                comboBoxIsSelectedByUser = false;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                App.trayIcon.Visible = false;
            }

            if (WindowState == WindowState.Minimized)
            {
                Hide();
                App.trayIcon.Visible = true;
            }
        }
    }
}