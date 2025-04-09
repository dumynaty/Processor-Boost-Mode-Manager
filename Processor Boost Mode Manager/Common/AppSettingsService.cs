namespace ProcessorBoostModeManager.Common
{
    public class AppSettingsService
    {
        public bool AutostartWithWindows { get; set; }
        public bool WindowsNotification { get; set; }
        public bool MinimizeToTray { get; set; }
        public string Theme { get; set; } = "Classic";
        public string BoostModes { get; set; } = "Disabled,Enabled,Aggressive";
        public int UpdateSpeed { get; set; }

        public void LoadSettings()
        {
            AutostartWithWindows = Properties.Settings.Default.AutostartWithWindows;
            WindowsNotification = Properties.Settings.Default.WindowsNotification;
            MinimizeToTray = Properties.Settings.Default.MinimizeToTray;
            Theme = Properties.Settings.Default.Theme;
            BoostModes = Properties.Settings.Default.BoostModes;
            UpdateSpeed = Properties.Settings.Default.UpdateSpeed;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.AutostartWithWindows = AutostartWithWindows;
            Properties.Settings.Default.WindowsNotification = WindowsNotification;
            Properties.Settings.Default.MinimizeToTray = MinimizeToTray;
            Properties.Settings.Default.Theme = Theme;
            Properties.Settings.Default.BoostModes = BoostModes;
            Properties.Settings.Default.UpdateSpeed = UpdateSpeed;

            Properties.Settings.Default.Save();
        }

        public void ResetSettings()
        {
            Properties.Settings.Default.AutostartWithWindows = false;
            Properties.Settings.Default.WindowsNotification = false;
            Properties.Settings.Default.MinimizeToTray = false;
            Properties.Settings.Default.Theme = "Classic";
            Properties.Settings.Default.BoostModes = "Disabled,Enabled,Aggressive";
            Properties.Settings.Default.UpdateSpeed = 5;

            Properties.Settings.Default.Save();
        }
    }
}
