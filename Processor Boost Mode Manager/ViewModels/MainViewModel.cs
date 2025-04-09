using ProcessorBoostModeManager.Commands;
using ProcessorBoostModeManager.Common;
using ProcessorBoostModeManager.Common.MenuItemsServices;
using ProcessorBoostModeManager.Common.shell32;
using ProcessorBoostModeManager.Enums;
using ProcessorBoostModeManager.Models.Poco;
using ProcessorBoostModeManager.ViewModels;
using RegistryManagerLibrary;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace ProcessorBoostModeManager.Views
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public RelayCommand ToggleThemeCommand => new RelayCommand(param =>
        {
            string theme = param as string ?? "Classic";
            ToggleTheme(theme);
            OnPropertyChanged();
        });
        public RelayCommand ToggleBoostModeCommand => new RelayCommand(param =>
        {
            string boostMode = (string)param;
            ToggleBoostMode(boostMode);
            OnPropertyChanged();
        });
        public RelayCommand ToggleUpdateSpeedCommand => new RelayCommand(param =>
        {
            int speed = (int)param;
            ToggleUpdateSpeed(speed);
            OnPropertyChanged();
        });
        public RelayCommand RefreshCommand => new RelayCommand(execute => UpdateProgram());
        public RelayCommand ResetSettingsCommand => new RelayCommand(execute => ResetSavedSettings());
        public RelayCommand ClearDatabaseCommand => new RelayCommand(execute => ClearDatabase());
        public RelayCommand AddCommand => new RelayCommand(execute => AddProgram());
        public RelayCommand AddManuallyCommand => new RelayCommand(execute => AddProgramManually());
        public RelayCommand RemoveCommand => new RelayCommand(execute => RemoveProgram());
        public RelayCommand OpenFileLocationCommand => new RelayCommand(execute => OpenFileLocation(), canExecute => SelectedProgram != null);

        private bool OnStartup = true;
        private string statusMessageUpper = string.Empty;
        private string statusMessageLower = string.Empty;
        private ProgramViewModel? selectedProgram = null;
        private int runningProgramsCount;
        private bool addButtonIsEnabled = true;

        public ObservableCollection<ProgramViewModel> ProgramsInUI { get; set; }
        public ICollectionView ProgramsView { get; private set; }
        public AppSettingsService AppSettingService { get; set; }
        public DatabaseService DatabaseService { get; private set; }
        public ProcessMonitorService ProcessMonitorService { get; private set; }
        public StatusMessageService StatusMessageService { get; set; }
        public ThemeService ThemeService { get; set; }
        public BoostModeService BoostModeMenuItemService { get; set; }
        public UpdateSpeedService UpdateSpeedService { get; set; }
        public DispatcherTimer Timer { get; private set; }
        public string AppName { get; private set; }
        public string AppPath { get; private set; }
        public string StatusMessageUpper 
        {
            get => statusMessageUpper; 
            set 
            {
                statusMessageUpper = value;
                OnPropertyChanged(); 
            }
        }
        public string StatusMessageLower 
        { 
            get => statusMessageLower; 
            set 
            { 
                statusMessageLower = value;
                OnPropertyChanged(); 
            } 
        }
        public ProgramViewModel? SelectedProgram 
        {
            get => selectedProgram; 
            set 
            {
                selectedProgram = value; 
                if (selectedProgram != null)
                    StatusMessageService.Lower($"Process selected: {selectedProgram.Name}", true, true);
                else
                    StatusMessageService.Lower($"Current mode: {(CPUBoostMode)CurrentBoostMode}", true, true);
                OnPropertyChanged();
            }
        }
        public int RunningProgramsCount
        {
            get => runningProgramsCount;
            private set
            {
                if (runningProgramsCount != value)
                {
                    runningProgramsCount = value;
                    StatusMessageService.Upper($"Currently running programs: {RunningProgramsCount}", true, true);
                }
            }
        }

        public bool AutostartWithWindows
        {
            get => AppSettingService.AutostartWithWindows;
            set
            {
                if (AppSettingService.AutostartWithWindows != value)
                {
                    AppSettingService.AutostartWithWindows = value;

                    ToggleAutostart(value);
                }
            }
        }
        public bool WindowsNotification
        {
            get => AppSettingService.WindowsNotification;
            set
            {
                if (AppSettingService.WindowsNotification != value)
                {
                    AppSettingService.WindowsNotification = value;

                    if (OnStartup == false)
                        ToggleWindowsNotification(AppSettingService.WindowsNotification);
                }
            }
        }
        public bool MinimizeToTray
        {
            get => AppSettingService.MinimizeToTray;
            set
            {
                if (AppSettingService.MinimizeToTray != value)
                {
                    AppSettingService.MinimizeToTray = value;
                }
            }
        }
        public string Theme 
        {
            get => AppSettingService.Theme;
            set
            {
                if (AppSettingService.Theme != value)
                {
                    AppSettingService.Theme = value;
                }
            }
        }
        public string BoostModes
        {
            get => AppSettingService.BoostModes;
            set
            {
                if (AppSettingService.BoostModes != value)
                {
                    AppSettingService.BoostModes = value;

                    AdjustComboBoxBoostModes();
                }
            }
        }
        public int UpdateSpeed 
        {
            get => AppSettingService.UpdateSpeed; 
            set
            {
                if (AppSettingService.UpdateSpeed != value)
                {
                    AppSettingService.UpdateSpeed = value;

                    RunTimer(true);
                }
            }
        }

        public bool AddButtonIsEnabled { get => addButtonIsEnabled; set { addButtonIsEnabled = value; OnPropertyChanged(); } }

        private CPUBoostMode currentBoostMode = CPUBoostMode.Disabled;
        public CPUBoostMode CurrentBoostMode
        {
            get => currentBoostMode;
            set
            {
                if (currentBoostMode != value)
                {
                    currentBoostMode = value;
                    if (SelectedProgram == null)
                        StatusMessageService.Lower($"Current mode: {CurrentBoostMode}", true, true);
                }
            }
        }

        public MainViewModel()
        {
            ProgramsInUI = new ObservableCollection<ProgramViewModel>();
            AppSettingService = new AppSettingsService();
            DatabaseService = new DatabaseService();
            ProcessMonitorService = new ProcessMonitorService();
            StatusMessageService = new StatusMessageService(this);
            ThemeService = new ThemeService();
            BoostModeMenuItemService = new BoostModeService();
            UpdateSpeedService = new UpdateSpeedService();
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

            AppSettingService.LoadSettings();
            ThemeService.SetMenuItemsTheme(Theme);
            BoostModeMenuItemService.SetMenuItemsSavedState(BoostModes);
            UpdateSpeedService.SetMenuItemsUpdateSpeed(UpdateSpeed);
            RegistryManager.GetActivePowerScheme();

            RunTimer(true);
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
            var (Database, HighestBoostMode, NewRunningProgramsCount) = ProcessMonitorService.GetProcessedDatabase(DatabaseService.PocoDatabase);
            bool refreshNeeded = false;

            // Initial Run to populate ProgramsInUI
            if (ProgramsInUI.Count == 0 && Database.Count != 0)
            {
                foreach (var program in Database)
                {
                    program.Icon = IconHandler.ExtractIcon(program.Location);
                    ProgramsInUI.Add(program);
                }
                AdjustComboBoxBoostModes();
            }

            // Check for changes in IsRunning / HighestValue
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
            // Add or Remove programs if needed
            else
            {
                string[] programsInUIArray = ProgramsInUI.Select(Program => Program.Name).ToArray();
                string[] programsInDBArray = Database.Select(Program => Program.Name).ToArray();

                if (ProgramsInUI.Count < Database.Count)
                {
                    foreach (var program in Database)
                    {
                        if (!programsInUIArray.Contains(program.Name))
                        {
                            program.Icon = IconHandler.ExtractIcon(program.Location);
                            ProgramsInUI.Add(program);
                            break;
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
                refreshNeeded = true;
            }

            if (refreshNeeded)
                ProgramsView.Refresh();

            int currentBoostMode = RegistryManager.GetProcessorBoostMode();
            ApplyBoostModeChange(currentBoostMode, HighestBoostMode);

            if (CurrentBoostMode != (CPUBoostMode)HighestBoostMode)
                CurrentBoostMode = (CPUBoostMode)HighestBoostMode;
            if (RunningProgramsCount != NewRunningProgramsCount)
                RunningProgramsCount = NewRunningProgramsCount;
        }
        private void ApplyBoostModeChange(int currentBoostMode, int HighestBoostMode)
        {
            if (currentBoostMode == HighestBoostMode)
                return;

            RegistryManager.SetProcessorBoostMode(HighestBoostMode);
            if (WindowsNotification == true)
                App.trayIcon.ShowBalloonTip(2500, "Status change:", $"Current mode set to: " +
                    $"{(CPUBoostMode)HighestBoostMode}", ToolTipIcon.None);
        }


        // ComboBoxes
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
            string[] validBoostModes = new string[]
            {
                "Disabled",                         // 0
                "Enabled",                          // 1
                "Aggressive",                       // 2
                "EfficientEnabled",                 // 3
                "EfficientAggressive",              // 4
                "AggressiveAtGuaranteed",           // 5
                "EfficientAggressiveAtGuaranteed"   // 6
            };

            // Find highest index / boost mode value in BoostModes
            int newHighestBoostMode = 0;
            foreach (var item in BoostModes.Split(','))
            {
                int index = Array.IndexOf(validBoostModes, item);

                if (index > newHighestBoostMode)
                {
                    newHighestBoostMode = index;
                }
            }

            // Apply new highest boost mode value
            foreach (var program in ProgramsInUI)
            {
                if ((int)program.BoostMode > newHighestBoostMode)
                {
                    program.BoostMode = (CPUBoostMode)newHighestBoostMode;
                }
            }
        }

        // Buttons
        private void AddProgram()
        {
            AddButtonIsEnabled = false;

            ProcessSelectionWindow selection = new ProcessSelectionWindow(this);
            selection.Show();
            selection.Activate();
        }
        private void AddProgramManually()
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
                DatabaseService.AddProgramToDatabase(program);
                UpdateProgram();

                StatusMessageService.Lower($"Program {program.Name} has been added to the Database!");
            }
        }
        private void RemoveProgram()
        {
            if (SelectedProgram != null)
            {
                string programName = SelectedProgram.Name;
                DatabaseService.RemoveProgramFromDatabase(SelectedProgram.Model);
                UpdateProgram();

                StatusMessageService.Lower($"Program {programName} has been removed!");
            }
            else
            {
                StatusMessageService.Lower("Please select a program to remove!");
            }
        }

        // Menu Items
        public void ToggleAutostart(bool isChecked)
        {
            if (isChecked)
            {
                if (RegistryManager.IsAppStartupEnabled(AppName) == false)
                {
                    RegistryManager.RegisterAppToStartup(AppName, AppPath);
                    StatusMessageService.Upper($"Application is registered to start with Windows!");
                }
            }
            else
            {
                if (RegistryManager.IsAppStartupEnabled(AppName) == true)
                {
                    RegistryManager.UnregisterAppFromStartup(AppName);
                    StatusMessageService.Upper($"Application is unregistered from starting with Windows!");
                }
            }
        }
        public void ToggleWindowsNotification(bool isChecked)
        {
            if (isChecked)
            {
                StatusMessageService.Upper($"Application will notify changes with Windows Baloon Pop-up!");
            }
            else
            {
                StatusMessageService.Upper($"Application will not notify changes with Windows Baloon Pop-up!");
            }
        }
        public void ToggleTheme(string selectedTheme)
        {
            Theme = selectedTheme;
            ThemeService.SetMenuItemsTheme(selectedTheme);
            Uri themeUri = new Uri($"Resources/Themes/{selectedTheme}.xaml", UriKind.Relative);

            ResourceDictionary newTheme = new ResourceDictionary() { Source = themeUri };
            App.Current.Resources.Clear();
            App.Current.Resources.MergedDictionaries.Add(newTheme);
        }
        public void ToggleBoostMode(string selectedBoostMode)
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
        }
        public void ToggleUpdateSpeed(int newUpdateSpeed)
        {
            UpdateSpeed = newUpdateSpeed;
            UpdateSpeedService.SetMenuItemsUpdateSpeed(newUpdateSpeed);
        }
        public void ResetSavedSettings()
        {
            AppSettingService.ResetSettings();
            AppSettingService.LoadSettings();

            ThemeService.SetMenuItemsTheme(Theme);
            BoostModeMenuItemService.SetMenuItemsSavedState(BoostModes);
            UpdateSpeedService.SetMenuItemsUpdateSpeed(UpdateSpeed);
        }
        public void ClearDatabase()
        {
            DatabaseService.CreateDatabase();
            UpdateProgram();
        }
        private void OpenFileLocation()
        {
            if (SelectedProgram != null)
            {
                string? programPath = SelectedProgram.Location;
                FileExplorer.ShowFileInExplorer(programPath);
                // Simpler but opens a new explorer.exe process every time it is called
                // System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{ programPath }\"");
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
