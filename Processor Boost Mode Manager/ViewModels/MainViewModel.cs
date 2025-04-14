using ProcessorBoostModeManager.Commands;
using ProcessorBoostModeManager.Common;
using ProcessorBoostModeManager.Common.MenuItemsServices;
using ProcessorBoostModeManager.Common.shell32;
using ProcessorBoostModeManager.Enums;
using ProcessorBoostModeManager.Models.Poco;
using RegistryManagerLibrary;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace ProcessorBoostModeManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public RelayCommand ToggleBoostModeCommand => new RelayCommand(param =>
        {
            string boostMode = (string)param;
            ToggleBoostMode(boostMode);
            AdjustComboBoxBoostModes();
        });
        public RelayCommand ToggleUpdateSpeedCommand => new RelayCommand(param =>
        {
            int newUpdateSpeed = (int)param;
            ToggleUpdateSpeed(newUpdateSpeed);
            ProcessMonitorService.RunTimer(true);
        });
        public RelayCommand RefreshCommand => new RelayCommand(execute => { ProgramsInUI.Clear(); ProcessMonitorService.UpdateProgram(); });
        public RelayCommand AppInfoCommand => new RelayCommand(execute => AccessGitHubRepo());
        public RelayCommand ResetSettingsCommand => new RelayCommand(execute => ResetSavedSettings());
        public RelayCommand ClearDatabaseCommand => new RelayCommand(execute => ClearDatabase());
        public RelayCommand AddCommand => new RelayCommand(execute => AddProgram());
        public RelayCommand AddManuallyCommand => new RelayCommand(execute => AddProgramManually());
        public RelayCommand RemoveCommand => new RelayCommand(execute => RemoveProgram());
        public RelayCommand OpenFileLocationCommand => new RelayCommand(execute => OpenFileLocation(), canExecute => SelectedProgram != null);

        
        private ProgramViewModel? selectedProgram;
        private int runningProgramsCount;
        private bool addButtonIsEnabled = true;

        public ObservableCollection<ProgramViewModel> ProgramsInUI { get; set; }
        public ICollectionView ProgramsView { get; private set; }
        public SavedSettingsService SavedSettingsService { get; private set; }
        public DatabaseService DatabaseService { get; private set; }
        public ProcessMonitorService ProcessMonitorService { get; private set; }
        public StatusMessageService StatusMessageService { get; set; } = new();
        public BoostModeService BoostModeMenuItemService { get; set; }
        public UpdateSpeedService UpdateSpeedService { get; set; }
        
        public string AppName { get; private set; }
        public string AppPath { get; private set; }
        
        public ProgramViewModel? SelectedProgram 
        {
            get => selectedProgram; 
            set 
            {
                selectedProgram = value; 
                if (selectedProgram != null)
                    StatusMessageService.Lower($"Process selected: {selectedProgram.Name}", true, true);
                else
                    StatusMessageService.Lower($"Current mode: {CurrentBoostMode}", true, true);
            }
        }
        public int RunningProgramsCount
        {
            get => runningProgramsCount;
            set
            {
                if (runningProgramsCount != value)
                {
                    runningProgramsCount = value;
                    StatusMessageService.Upper($"Currently running programs: {RunningProgramsCount}", true, true);
                }
            }
        }
        public bool AddButtonIsEnabled { get => addButtonIsEnabled; set { addButtonIsEnabled = value; OnPropertyChanged(); } }

        private CPUBoostMode currentBoostMode;
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
        public bool DatabaseIsEmpty;

        // Constructor
        public MainViewModel()
        {
            AppName = Assembly.GetExecutingAssembly().GetName().Name ?? "Processor Boost Mode Manager";
            AppPath = Environment.ProcessPath ?? string.Empty;

            ProgramsInUI = new ObservableCollection<ProgramViewModel>();
            SavedSettingsService = new SavedSettingsService(this);
            DatabaseService = new DatabaseService();
            ProcessMonitorService = new ProcessMonitorService(this);
            StatusMessageService = new StatusMessageService();
            BoostModeMenuItemService = new BoostModeService();
            UpdateSpeedService = new UpdateSpeedService();

            ProgramsView = CollectionViewSource.GetDefaultView(ProgramsInUI);
            {
                ProgramsView.SortDescriptions.Add(new SortDescription("HighestValue", ListSortDirection.Descending));
                ProgramsView.SortDescriptions.Add(new SortDescription("IsRunning", ListSortDirection.Descending));
                ProgramsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                ProgramsView.Refresh();
            }
            
            BoostModeMenuItemService.SetMenuItemsSavedState(SavedSettingsService.BoostModes);
            UpdateSpeedService.SetMenuItemsUpdateSpeed(SavedSettingsService.UpdateSpeed);
            RegistryManager.GetActivePowerScheme();
            CurrentBoostMode = (CPUBoostMode)RegistryManager.GetProcessorBoostMode();

            StartProgram();
        }

        // Initialization of program logic
        private void StartProgram()
        {
            ProcessMonitorService.RunTimer(true);

            if (DatabaseIsEmpty)
            {
                StatusMessageService.Upper($"Database Empty!", true, true);
                StatusMessageService.Lower($"Add a program to the list!", true, true);
            }
            else
            {
                // Ensure StatusMessageLower gets set to showing the CurrentBoostMode at program start
                SelectedProgram = null;
            }
        }

        // Menu Items
        public void ToggleBoostMode(string selectedBoostMode)
        {
            string currentBoostModes = SavedSettingsService.BoostModes;
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
                if (currentBoostModes.Split(',').Length < 2)
                {
                    BoostModeMenuItemService.ToggleMenuItemState(selectedBoostMode, true);
                    return;
                }
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
            SavedSettingsService.BoostModes = currentBoostModes;
        }
        public void ToggleUpdateSpeed(int newUpdateSpeed)
        {
            SavedSettingsService.UpdateSpeed = newUpdateSpeed;
            UpdateSpeedService.SetMenuItemsUpdateSpeed(newUpdateSpeed);
        }
        private void AccessGitHubRepo()
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
        public void ResetSavedSettings()
        {
            SavedSettingsService.ResetSettings();
            SavedSettingsService.LoadSettings();

            BoostModeMenuItemService.SetMenuItemsSavedState(SavedSettingsService.BoostModes);
            UpdateSpeedService.SetMenuItemsUpdateSpeed(SavedSettingsService.UpdateSpeed);

            AdjustComboBoxBoostModes();
            ProcessMonitorService.RunTimer(true);
        }
        public void ClearDatabase()
        {
            DatabaseService.PocoDatabase.Clear();
            DatabaseService.CreateDatabase();
            ProgramsInUI.Clear();
            ProcessMonitorService.UpdateProgram();
            RunningProgramsCount = 0;

            StartProgram();
        }
        private void OpenFileLocation()
        {
            if (SelectedProgram != null)
            {
                string? programPath = SelectedProgram.Location;
                if (!File.Exists(programPath))
                    System.Windows.MessageBox.Show($"Check program location: {SelectedProgram.Location}", "Program not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                FileExplorer.ShowFileInExplorer(programPath);
                // Simpler but opens a new explorer.exe process every time it is called
                // System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{ programPath }\"");
            }
        }

        // CheckBoxes
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
                ProcessMonitorService.UpdateProgram();

                StatusMessageService.Lower($"Program {program.Name} has been added to the Database!");
            }
        }
        private void RemoveProgram()
        {
            if (SelectedProgram != null)
            {
                string programName = SelectedProgram.Name;
                DatabaseService.RemoveProgramFromDatabase(SelectedProgram.Model);
                ProcessMonitorService.UpdateProgram();

                StatusMessageService.Lower($"Program {programName} has been removed!");
            }
            else
            {
                StatusMessageService.Lower("Please select a program to remove!");
            }
        }

        // ComboBoxes
        public void AdjustComboBoxBoostModes()
        {
            foreach (var program in ProgramsInUI)
            {
                program.ComboBoxSelection.SetSavedComboBoxItems(SavedSettingsService.BoostModes);
            }

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
            foreach (var item in SavedSettingsService.BoostModes.Split(','))
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


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
