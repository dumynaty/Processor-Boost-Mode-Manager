using ProcessorBoostModeManager.Common;
using ProcessorBoostModeManager.Enums;
using ProcessorBoostModeManager.Models;
using ProcessorBoostModeManager.ViewModels;
using RegistryManagerLibrary;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace ProcessorBoostModeManager.Views
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string statusMessageUpper = string.Empty;
        private string statusMessageLower = string.Empty;
        private ProgramViewModel? selectedProgram = null;
        private bool addButtonIsEnabled = true;

        public ObservableCollection<ProgramViewModel> ProgramsInUI { get; set; }
        public ICollectionView ProgramsView { get; private set; }
        public DatabaseService DatabaseJSON { get; private set; }
        public ProcessMonitorService ProcessMonitor { get; private set; }
        public StatusMessageService StatusMessageService { get; set; }
        public DispatcherTimer Timer { get; private set; }
        public string AppName { get; private set; }
        public string AppPath { get; private set; }
        public int RunningProgramsCount { get; private set; }
        public string StatusMessageUpper { get => statusMessageUpper; set { statusMessageUpper = value; OnPropertyChanged(); } }
        public string StatusMessageLower { get => statusMessageLower; set { statusMessageLower = value; OnPropertyChanged(); } }
        public ProgramViewModel? SelectedProgram { get => selectedProgram; set { selectedProgram = value; OnPropertyChanged(); } }

        public bool AutostartWithWindows { get; set; }
        public bool WindowsNotification { get; set; }
        public bool MinimizeToTray { get; set; }
        public string Theme { get; set; } = "Classic";
        public string BoostModes { get; set; } = "Disabled,Enabled,Aggressive";
        public int UpdateSpeed { get; set; }

        public bool AddButtonIsEnabled { get => addButtonIsEnabled; set { addButtonIsEnabled = value; OnPropertyChanged(); } }

        public MainViewModel()
        {
            ProgramsInUI = new ObservableCollection<ProgramViewModel>();
            DatabaseJSON = new DatabaseService();
            ProcessMonitor = new ProcessMonitorService();
            StatusMessageService = new StatusMessageService(this);
            Timer = new DispatcherTimer();

            ProgramsView = CollectionViewSource.GetDefaultView(ProgramsInUI);
            {
                ProgramsView.SortDescriptions.Add(new SortDescription("HighestValue", ListSortDirection.Descending));
                ProgramsView.SortDescriptions.Add(new SortDescription("IsRunning", ListSortDirection.Descending));
                ProgramsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                ProgramsView.Refresh();
            }
            
            AppName = Assembly.GetExecutingAssembly().GetName().Name ?? "Processor Boost Mode Manager";
            AppPath = Environment.ProcessPath ?? string.Empty;

            StatusMessageUpper = "Awaiting settings status..";
            StatusMessageLower = "Awaiting database status..";

            RegistryManager.GetActivePowerScheme();

            InitialSetttings();

            UpdateProgram();
            RunTimer();
        }

        private void InitialSetttings()
        {
            AutostartWithWindows = Properties.Settings.Default.AutostartWithWindows;
            WindowsNotification = Properties.Settings.Default.WindowsNotification;
            MinimizeToTray = Properties.Settings.Default.MinimizeToTray;
            Theme = Properties.Settings.Default.Theme;
            BoostModes = Properties.Settings.Default.BoostModes;
            UpdateSpeed = Properties.Settings.Default.UpdateSpeed;
        }

        public void RunTimer(bool restart = false)
        {
            if (restart == true)
            {
                UpdateProgram();
                Timer.Stop();
            }

            Timer.Interval = TimeSpan.FromSeconds(UpdateSpeed);
            Timer.Tick += (s, e) =>
            {
                UpdateProgram();
            };
            Timer.Start();
        }
        public void UpdateProgram()
        {
            var (Database, HighestBoostMode) = ProcessMonitor.GetDatabaseProcessesOC(DatabaseJSON.GetDatabaseProcesses().ToList());
            bool refreshNeeded = false;
            if (ProgramsInUI.Count == 0 && Database.Count != 0)
            {
                foreach (var program in Database)
                {
                    ProgramsInUI.Add(program);
                }
            }

            if (ProgramsInUI.Count == Database.Count)
            {
                for (int i = 0; i < Database.Count; i++)
                {
                    if (ProgramsInUI[i].IsRunning != Database[i].IsRunning)
                    {
                        ProgramsInUI[i].IsRunning = Database[i].IsRunning;
                        refreshNeeded = true;
                    }
                    if (ProgramsInUI[i].HighestValue != Database[i].HighestValue)
                    {
                        ProgramsInUI[i].HighestValue = Database[i].HighestValue;
                        refreshNeeded = true;
                    }
                }
            }
            else
            {
                refreshNeeded = true;

                string[] programsInUIArray = ProgramsInUI.Select(Program => Program.Name).ToArray();
                string[] programsInDBArray = Database.Select(Program => Program.Name).ToArray();

                if (ProgramsInUI.Count < Database.Count)
                {
                    foreach (var program in Database)
                    {
                        if (!programsInUIArray.Contains(program.Name))
                        {
                            ProgramsInUI.Add(program);
                        }
                    }
                }
                else if (ProgramsInUI.Count > Database.Count)
                {
                    foreach (var program in ProgramsInUI)
                    {
                        if (!programsInDBArray.Contains(program.Name))
                        {
                            ProgramsInUI.Remove(program);
                            break;
                        }
                    }
                }
            }

            if (refreshNeeded)
                ProgramsView.Refresh();

            int currentBoostMode = RegistryManager.GetProcessorBoostMode();
            if (currentBoostMode != HighestBoostMode)
            {
                try
                {
                    RegistryManager.SetProcessorBoostMode(HighestBoostMode);
                    currentBoostMode = HighestBoostMode;
                    if (WindowsNotification == true)
                        App.trayIcon.ShowBalloonTip(2500, "Status change:", $"Current mode set to: " +
                            $"{(CPUBoostMode)HighestBoostMode}", ToolTipIcon.None);
                }
                catch (Exception e)
                {
                    throw new Exception($"Processor boost mode change failed! {e.Message}");
                }
            }

            RunningProgramsCount = ProgramsInUI.Count(p => p.IsRunning);
            StatusMessageService.Upper($"Currently running programs: {RunningProgramsCount}", true, true);

            if (SelectedProgram != null)
                StatusMessageService.Lower($"Process selected: {SelectedProgram.Name}", true);
            else
                StatusMessageService.Lower($"Current mode: {(CPUBoostMode)currentBoostMode}", true, true);
        }
        private void AdjustComboBoxBoostModes()
        {
            string[] availableBoostModes = BoostModes.Split(',');
            foreach (var program in ProgramsInUI)
            {
                if (availableBoostModes.Contains(nameof(program.Disabled)))
                    program.Disabled = Visibility.Visible;
                else program.Disabled = Visibility.Collapsed;
                if (availableBoostModes.Contains(nameof(program.Enabled)))
                    program.Enabled = Visibility.Visible;
                else program.Enabled = Visibility.Collapsed;
                if (availableBoostModes.Contains(nameof(program.Aggressive)))
                    program.Aggressive = Visibility.Visible;
                else program.Aggressive = Visibility.Collapsed;
                if (availableBoostModes.Contains(nameof(program.EfficientEnabled)))
                    program.EfficientEnabled = Visibility.Visible;
                else program.EfficientEnabled = Visibility.Collapsed;
                if (availableBoostModes.Contains(nameof(program.EfficientAggressive)))
                    program.EfficientAggressive = Visibility.Visible;
                else program.EfficientAggressive = Visibility.Collapsed;
                if (availableBoostModes.Contains(nameof(program.AggressiveAtGuaranteed)))
                    program.AggressiveAtGuaranteed = Visibility.Visible;
                else program.AggressiveAtGuaranteed = Visibility.Collapsed;
                if (availableBoostModes.Contains(nameof(program.EfficientAggressiveAtGuaranteed)))
                    program.EfficientAggressiveAtGuaranteed = Visibility.Visible;
                else program.EfficientAggressiveAtGuaranteed = Visibility.Collapsed;
            }

            AdjustProgramBoostModeValuesAfterComboBoxChanges();
        }
        private void AdjustProgramBoostModeValuesAfterComboBoxChanges()
        {
            int newHighestBoostMode = 0;

            foreach (var item in BoostModes.Split(','))
            {
                if (item == "Disabled" && newHighestBoostMode < (int)CPUBoostMode.Disabled)
                    newHighestBoostMode = (int)CPUBoostMode.Disabled;
                if (item == "Disabled" && newHighestBoostMode < (int)CPUBoostMode.Disabled)
                    newHighestBoostMode = (int)CPUBoostMode.Disabled;
                if (item == "Enabled" && newHighestBoostMode < (int)CPUBoostMode.Enabled)
                    newHighestBoostMode = (int)CPUBoostMode.Enabled;
                if (item == "Aggressive" && newHighestBoostMode < (int)CPUBoostMode.Aggressive)
                    newHighestBoostMode = (int)CPUBoostMode.Aggressive;
                if (item == "EfficientEnabled" && newHighestBoostMode < (int)CPUBoostMode.EfficientEnabled)
                    newHighestBoostMode = (int)CPUBoostMode.EfficientEnabled;
                if (item == "EfficientAggressive" && newHighestBoostMode < (int)CPUBoostMode.EfficientAggressive)
                    newHighestBoostMode = (int)CPUBoostMode.EfficientAggressive;
                if (item == "AggressiveAtGuaranteed" && newHighestBoostMode < (int)CPUBoostMode.AggressiveAtGuaranteed)
                    newHighestBoostMode = (int)CPUBoostMode.AggressiveAtGuaranteed;
                if (item == "EfficientAggressiveAtGuaranteed" && newHighestBoostMode < (int)CPUBoostMode.EfficientAggressiveAtGuaranteed)
                    newHighestBoostMode = (int)CPUBoostMode.EfficientAggressiveAtGuaranteed;
            }

            bool needSaving = false;
            foreach (var program in ProgramsInUI)
            {
                if ((int)program.BoostMode > newHighestBoostMode)
                {
                    program.BoostMode = (CPUBoostMode)newHighestBoostMode;
                    needSaving = true;
                }
            }
            if (needSaving)
            {
                DatabaseJSON.SaveDatabase(ProgramsInUI.ToList());
            }
        }

        public void AddProgram()
        {
            AddButtonIsEnabled = false;
            ProcessSelectionWindow selection = new ProcessSelectionWindow(this);
            selection.Show();
            selection.Activate();
        }
        public void AddProgramManually()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "Select an Application",
                Filter = "Executable files (*.exe)|*.exe" + "|All Files|*.*",
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                string fileLocation = openFileDialog.FileName;
                var program = new ProgramModel
                {
                    Name = fileName,
                    Location = fileLocation
                };
                DatabaseJSON.AddProgramToDatabase(program);
                UpdateProgram();

                StatusMessageService.Lower($"Program {program.Name} has been added to the Database!");
            }
        }
        public void RemoveProgram(ProgramViewModel selectedProgram)
        {
            DatabaseJSON.RemoveProgramFromDatabase(selectedProgram.Model);
            UpdateProgram();

            StatusMessageService.Lower($"Program {selectedProgram.Name} has been removed!");
        }

        public void ToggleAutostart(bool isChecked)
        {
            if (isChecked)
            {
                if (RegistryManager.IsAppStartupEnabled(AppPath, AppName) == false)
                {
                    RegistryManager.RegisterAppToStartup(AppName, AppPath);
                    StatusMessageService.Upper($"Application is registered to start with Windows!");
                }
            }
            else
            {
                RegistryManager.UnregisterAppFromStartup(AppName, AppPath);
                StatusMessageService.Upper($"Application is unregistered from starting with Windows!");
            }
            
            AutostartWithWindows = isChecked;
        }
        public void ToggleWindowsNotification(bool isChecked)
        {
            WindowsNotification = isChecked;

            if (isChecked)
            {
                StatusMessageService.Upper($"Application will notify changes with Windows Baloon Pop-up!");
            }
            else
            {
                StatusMessageService.Upper($"Application will not notify changes with Windows Baloon Pop-up!");
            }
        }
        public void ToggleMinimizeToTray(bool isChecked)
        {
            MinimizeToTray = isChecked;
        }
        public void ToggleTheme(string selectedTheme, Uri themeUri)
        {
            Theme = selectedTheme;

            ResourceDictionary newTheme = new ResourceDictionary() { Source = themeUri };
            App.Current.Resources.Clear();
            App.Current.Resources.MergedDictionaries.Add(newTheme);
        }
        public void SelectBoostModes(string selectedBoostMode)
        {
            string currentBoostModes = BoostModes;
            string boostMode = selectedBoostMode;

            

            if (currentBoostModes.Split(',').Contains(boostMode))
            {
                currentBoostModes = currentBoostModes.Replace(boostMode, "");
                if (currentBoostModes[0] == ',')
                {
                    currentBoostModes = currentBoostModes.Remove(0, 1);
                }
                if (currentBoostModes[currentBoostModes.Length - 1] == ',')
                {
                    currentBoostModes = currentBoostModes.Remove(currentBoostModes.Length - 1);
                }
                if (currentBoostModes.Contains(",,"))
                {
                    currentBoostModes = currentBoostModes.Replace(",,", ",");
                }

                // Make sure at least 2 ComboBoxes stay selected
                if (currentBoostModes.Split(',').Count() < 2)
                    return;
            }
            else
            {
                if (currentBoostModes[currentBoostModes.Length - 1] != ',')
                {
                    currentBoostModes += $",{boostMode}";
                }
                else
                {
                    currentBoostModes += $"{boostMode}";
                }
            }
            BoostModes = currentBoostModes;

            AdjustComboBoxBoostModes();
        }

        

        public void ToggleUpdateSpeed(int updateSpeed)
        {
            UpdateSpeed = updateSpeed;
            RunTimer(true);
        }

        public void SaveAppSettingsProperties()
        {
            Properties.Settings.Default.AutostartWithWindows = AutostartWithWindows;
            Properties.Settings.Default.WindowsNotification = WindowsNotification;
            Properties.Settings.Default.MinimizeToTray = MinimizeToTray;
            Properties.Settings.Default.Theme = Theme;
            Properties.Settings.Default.BoostModes = BoostModes;
            Properties.Settings.Default.UpdateSpeed = UpdateSpeed;

            Properties.Settings.Default.Save();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
