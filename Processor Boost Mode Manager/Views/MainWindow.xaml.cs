// NEED TO ADD
//
// Logging file
// Database backup and recovery
// Startup Arguments


using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.Versioning;
using ProcessorBoostModeManager.ViewModels;
using ProcessorBoostModeManager.Views;
using System.Diagnostics;
using ComboBox = System.Windows.Controls.ComboBox;

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
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            _mainViewModel.AppSettingService.ResetSettings();
        }

        // Menu Items
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

        //ComboBoxes
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _mainViewModel.DatabaseService.SaveDatabase(_mainViewModel.ProgramsInUI.Select(p => p.Model).ToList());
            _mainViewModel.UpdateProgram();
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
