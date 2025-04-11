using ProcessorBoostModeManager.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProcessorBoostModeManager.Common
{
    public class SavedSettingsService : INotifyPropertyChanged
    {
        MainViewModel _model;
        public SavedSettingsService(MainViewModel model)
        {
            _model = model;
            LoadSettings();
        }

        private bool autostartWithWindows;
        private bool windowsNotification;
        private bool minimizeToTray;
        private string theme = "Classic";
        private string boostModes = "Disabled,Enabled,Aggressive";
        private int updateSpeed;
        public bool AutostartWithWindows
        {
            get => autostartWithWindows;
            set
            {
                if (autostartWithWindows != value)
                {
                    autostartWithWindows = value;
                    _model.ToggleAutostart(value);
                    OnPropertyChanged();
                }
            }
        }
        public bool WindowsNotification
        {
            get => windowsNotification;
            set
            {
                if (windowsNotification != value)
                {
                    windowsNotification = value;
                    _model.ToggleWindowsNotification(value);
                    OnPropertyChanged();
                }
            }
        }
        public bool MinimizeToTray
        {
            get => minimizeToTray;
            set
            {
                if (minimizeToTray != value)
                {
                    minimizeToTray = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Theme
        {
            get => theme;
            set
            {
                if (theme != value)
                {
                    theme = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BoostModes
        {
            get => boostModes;
            set
            {
                if (boostModes != value)
                {
                    boostModes = value;
                    OnPropertyChanged();
                }
            }
        }
        public int UpdateSpeed
        {
            get => updateSpeed;
            set
            {
                if (updateSpeed != value)
                {
                    updateSpeed = value;
                    OnPropertyChanged();
                }
            }
        }
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


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
