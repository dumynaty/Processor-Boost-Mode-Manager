// NEED TO ADD
//
// Logging file
// Error Handlings


using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using ProcessorBoostModeManager.ViewModels;
using ComboBox = System.Windows.Controls.ComboBox;

namespace ProcessorBoostModeManager
{
    public partial class MainWindow : Window
    {
        public MainViewModel _mainViewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _mainViewModel;
        }

        // Menu Items
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ListBox
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
                if (originalSource is ComboBox || ProcessListBox.SelectedItem is not ProgramViewModel)
                {
                    e.Handled = true;
                    return;
                }
                originalSource = VisualTreeHelper.GetParent(originalSource);
            }
        }

        // Window State
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _mainViewModel.SavedSettingsService.MinimizeToTray == true)
            {
                Hide();
            }
        }
    }
}
