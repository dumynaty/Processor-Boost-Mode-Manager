using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace ProcessorBoostModeManager
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
                mainWindow.Hide();
            else
                mainWindow.Show();
            GUIDHandling.InitialSetup();

            TrayIconInitialization();
        }

        private void TrayIconInitialization()
        {
            var appIcon = Assembly.GetExecutingAssembly().GetManifestResourceStream("ProcessorBoostModeManager.Icons.Processor Boost Mode Manager.ico");
            if (appIcon != null)
                trayIcon.Icon = new Icon(appIcon);
            trayIcon.ContextMenuStrip = new ContextMenuStrip();
            var openIcon = Assembly.GetExecutingAssembly().GetManifestResourceStream("ProcessorBoostModeManager.Icons.Processor Boost Mode Manager.ico");
            if (openIcon != null)
                trayIcon.ContextMenuStrip.Items.Add("Open", new Icon(openIcon).ToBitmap(), OnOpenIconClicked);
            var openFileLocationIcon = Assembly.GetExecutingAssembly().GetManifestResourceStream("ProcessorBoostModeManager.Icons.File.ico");
            if (openFileLocationIcon != null)
                trayIcon.ContextMenuStrip.Items.Add("Open file location", new Icon(openFileLocationIcon).ToBitmap(), OnOpenFileLocationIconClicked);
            var exitIcon = Assembly.GetExecutingAssembly().GetManifestResourceStream("ProcessorBoostModeManager.Icons.Shutdown.ico");
            if (exitIcon != null)
                trayIcon.ContextMenuStrip.Items.Add("Close", new Icon(exitIcon).ToBitmap(), OnExitIconClicked);

            trayIcon.Text = "Processor Boost Mode Manager";
            trayIcon.Visible = true;
            trayIcon.MouseClick += TrayIcon_MouseClick;
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

                // This doesn't shutdown
                App.Current.Shutdown();
            }
        }
        private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Activate();
            }
            if (e.Button == MouseButtons.Middle)
            {
                App.Current?.Shutdown();
            }
        }
        private void OnOpenIconClicked(object? sender, EventArgs e)
        {
            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
        }
        private void OnOpenFileLocationIconClicked(object? sender, EventArgs e)
        {
            string? programPath = Environment.ProcessPath;
            if (programPath != null)
                FileExplorer.ShowFileInExplorer(programPath);
        }
        private void OnExitIconClicked(object? sender, EventArgs e)
        {
            App.Current.Shutdown();
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
