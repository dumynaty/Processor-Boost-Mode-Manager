using ProcessorBoostModeManager.Common;
using ProcessorBoostModeManager.Common.shell32;
using ProcessorBoostModeManager.Models.Poco;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ProcessorBoostModeManager.ViewModels
{
    public class ProcessSelectionViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ProgramViewModel> ProgramsInUI { get; set; }
        private ProgramViewModel? _selectedProcess = null;
        public ProgramViewModel? SelectedProcess
        { 
            get => _selectedProcess; 
            set 
            {
                _selectedProcess = value;
                OnPropertyChanged();
            }
        }

        private MainViewModel _model;
        public ProcessSelectionViewModel(MainViewModel MainViewModel)
        {
            _model = MainViewModel;

            ProgramsInUI = new ObservableCollection<ProgramViewModel>();
            LoadProcesses();
        }

        private void LoadProcesses()
        {
            var excludedPaths = new[]
            {
                @"C:\Windows\System32\",
                @"C:\Windows\SystemApps\",
                @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\ServiceHub\"
            };

            var runningProcesses = Process.GetProcesses().Distinct().OrderBy(p => p.ProcessName).GroupBy(p => p.ProcessName).Select(g => g.First()).ToList();

            foreach (var process in runningProcesses)
            {
                try
                {
                    string? selectedProcessLocation = process.MainModule?.FileName;
                    if (selectedProcessLocation != null)
                    {
                        if (excludedPaths.Any(path => selectedProcessLocation.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                            continue;

                        var program = new ProgramViewModel(new ProgramModel { Name = process.ProcessName, Location = selectedProcessLocation })
                        {
                            Icon = IconHandler.ExtractIcon(selectedProcessLocation)
                        };
                        ProgramsInUI.Add(program);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
        public bool AddProgramToDatabase()
        {
            if (SelectedProcess == null)
                return false;

            try
            {
                _model.DatabaseService.AddProgramToDatabase(SelectedProcess.Model);
                _model.StatusMessageService.Lower($"Program {SelectedProcess.Name} has been added to the Database!");
                _model.ProcessMonitorService.UpdateProgram();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't add Windows protected process! {ex.Message}");
            }
        }
        public void OpenFileLocation()
        {
            if (SelectedProcess != null)
            {
                string? programPath = SelectedProcess.Location;
                FileExplorer.ShowFileInExplorer(programPath);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
