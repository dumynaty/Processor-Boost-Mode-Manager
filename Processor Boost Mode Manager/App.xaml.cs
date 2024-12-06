using ProcessBoostModeManager;
using System.Diagnostics;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace Processor_Boost_Mode_Manager
{
    public partial class App : System.Windows.Application
    {
        public readonly static NotifyIcon trayIcon = new();
        public static MainWindow? MainWindowInstance { get; set; }
        public static ProcessSelectionWindow? ProcessSelectionInstance { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InstanceCheck();

            MainWindow mainWindow = new();
            if (mainWindow.AutostartCheckBox.IsChecked == true)
            {
                trayIcon.Visible = true;
                mainWindow.Hide();
            }
            else
            {
                trayIcon.Visible = false;
                mainWindow.Show();
            }

            trayIcon.Text = "Processor Boost Mode Manager";
            trayIcon.Icon = new Icon("C:\\Users\\Windows G\\source\\repos\\-- ICON --\\Processor Boost Mode Manager.ico");
            trayIcon.MouseClick += TrayIcon_MouseClick;
        }

        private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Activate();
            }
            if (e.Button == MouseButtons.Right)
            {
                App.Current.Shutdown();
            }
        }

        private static void InstanceCheck()
        {
            string currentProcessName = Process.GetCurrentProcess().ProcessName;
            int matchingProcesses = Process.GetProcesses().Count(p => p.ProcessName.Equals(currentProcessName, StringComparison.OrdinalIgnoreCase));

            if (matchingProcesses > 1)
            {
                MessageBox.Show("Only one instance of Processor Boost Mode Manager can run at a time!",
                                "Processor Boost Mode Manager",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                App.Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon?.Dispose();
            if (MainWindowInstance != null)
            {
                List<ProgramModel> displayedList = MainWindowInstance.ProgramsInUI.ToList();
                JsonService.SavePrograms(displayedList);
            }
            base.OnExit(e);
        }
    }
}
