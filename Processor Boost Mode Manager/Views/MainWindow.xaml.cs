// NEED TO ADD
//
// Logging file
// Database backup and recovery
// Startup Arguments


using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using ComboBox = System.Windows.Controls.ComboBox;
using ProcessorBoostModeManager.Common.shell32;
using System.Runtime.Versioning;
using ProcessorBoostModeManager.ViewModels;
using ProcessorBoostModeManager.Views;
using System.ComponentModel;
using System.Diagnostics;

namespace ProcessorBoostModeManager
{
    [SupportedOSPlatform("windows")]
    public partial class MainWindow : Window
    {
        public MainViewModel _mainViewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _mainViewModel;

            App.MainWindowInstance = this;

            InitializeMenuItems();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void InitializeMenuItems()
        {
            foreach (MenuItem item in ThemeMenuItems.Items)
            {
                if (_mainViewModel.Theme.Contains((string)item.Header))
                {
                    item.IsChecked = true;
                }
            }

            foreach (MenuItem item in BoostModesMenuItems.Items)
            {
                if (_mainViewModel.BoostModes.Contains((string)item.Header))
                {
                    item.IsChecked = true;
                }
            }

            foreach (MenuItem item in UpdateSpeedMenuItems.Items)
            {
                if (_mainViewModel.UpdateSpeed == int.Parse((string)item.Tag))
                {
                    item.IsChecked = true;
                    break;
                }
            }
        }

        // Menu Items
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ChangedTheme_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem selectedTheme)
            {
                _mainViewModel.ToggleTheme((string)selectedTheme.Header, new Uri($"Resources/Themes/{(string)selectedTheme.Header}.xaml", UriKind.Relative));

                foreach (MenuItem item in ThemeMenuItems.Items)
                {
                    if (item.IsChecked != ((string)item.Header == _mainViewModel.Theme))
                        item.IsChecked = ((string)item.Header == _mainViewModel.Theme);
                }
            }
        }
        private void BoostModeItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem selectedBoostMode)
            {
                _mainViewModel.SelectBoostModes((string)selectedBoostMode.Header);

                foreach (MenuItem boostMode in BoostModesMenuItems.Items)
                {
                    if (boostMode.IsChecked != _mainViewModel.BoostModes.Split(',').Contains((string)boostMode.Header))
                        boostMode.IsChecked = _mainViewModel.BoostModes.Split(',').Contains((string)boostMode.Header);
                }
            }
        }
        private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _mainViewModel.UpdateProgram();
        }
        private void UpdateSpeedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem selectedUpdateSpeed)
            {
                int updateSpeed = int.Parse((string)selectedUpdateSpeed.Tag);
                _mainViewModel.UpdateSpeed = updateSpeed;

                foreach (MenuItem item in UpdateSpeedMenuItems.Items)
                {
                    if ((string)item.Header != (string)selectedUpdateSpeed.Header)
                        item.IsChecked = ((string)item.Header == (string)selectedUpdateSpeed.Header);
                }
            }
        }
        private void AppInfo_Click(object sender, RoutedEventArgs e)
        {
            const string readmeUrl = "https://github.com/dumynaty/Processor-Boost-Mode-Manager/blob/master/README.md";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = readmeUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                System.Windows.MessageBox.Show("Visit github.com/dumynaty/Processor-Boost-Mode-Manager/" +
                    " read the README.md file or report your issue.", "Error accessing website!"
                    , MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Buttons
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _mainViewModel.AddProgram();
        }
        private void AddManuallyButton_Click(object sender, RoutedEventArgs e)
        {
            _mainViewModel.AddProgramManually();
        }
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListBox.SelectedItem is ProgramViewModel selectedProgram)
            {
                _mainViewModel.RemoveProgram(selectedProgram);
            }
            else
            {
                _mainViewModel.StatusMessageService.Lower("Please select a program to remove!");
            }
        }

        // ListBox
        // !!! --- Implement this method in ProcessSelectionWindow and see if MouseBindings will work --- !!!
        private void ProcessListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        private void ProcessListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Deselect ListBox Item if the Right Button is pressed outside an Item
            var clickedItem = ProcessListBox.ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
            if (clickedItem == null)
            {
                ProcessListBox.SelectedItem = null;
                e.Handled = true;
            }
        }
        private void ProcessListBox_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            // Don't open context menu if right clicked on the ComboBox
            var originalSource = e.OriginalSource as DependencyObject;
            while (originalSource != null)
            {
                if (originalSource is ComboBox || ProcessListBox.SelectedItem is not ProgramViewModel selectedProcess)
                {
                    e.Handled = true;
                    return;
                }
                originalSource = VisualTreeHelper.GetParent(originalSource);
            }
        }
        private void OpenFileLocationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListBox.SelectedItem is ProgramViewModel selectedProcess)
            {
                string? programPath = selectedProcess.Location;
                FileExplorer.ShowFileInExplorer(programPath);
                // Simpler but opens a new explorer.exe process every time it is called
                // System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{ programPath }\"");
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
                _mainViewModel.DatabaseJSON.SaveDatabase(_mainViewModel.ProgramsInUI.ToList());
                _mainViewModel.UpdateProgram();
            }
        }
        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (System.Windows.Input.Mouse.DirectlyOver is ComboBox comboBox)
            {
                comboBox.IsDropDownOpen = true;
            }
        }

        // Window State
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _mainViewModel.MinimizeToTray == true)
            {
                Hide();
            }
        }
    }
}
