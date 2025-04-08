using ProcessorBoostModeManager.Common;
using ProcessorBoostModeManager.Models;
using ProcessorBoostModeManager.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ProcessorBoostModeManager.ViewModels
{
    public class ProcessSelectionViewModel : INotifyPropertyChanged
    {
        private MainViewModel _mainViewModel;
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
        
        public ProcessSelectionViewModel(MainViewModel MainViewModel)
        {
            _mainViewModel = MainViewModel;

            ProgramsInUI = new ObservableCollection<ProgramViewModel>();
            LoadProcesses();
        }

        private void LoadProcesses()
        {
            var runningProcesses = Process.GetProcesses().Where(p =>
            {
                try
                {
                    string? filePath = p.MainModule?.FileName;
                    return !string.IsNullOrEmpty(filePath) &&
                           !filePath.StartsWith(@"C:\Windows\", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }).OrderBy(p => p.ProcessName).GroupBy(p => p.ProcessName).Select(g => g.First()).ToList();

            foreach (var process in runningProcesses)
            {
                try
                {
                    string selectedProcessLocation = process.MainModule?.FileName ?? "Unknown Location";
                    var program = new ProgramViewModel(new ProgramModel { Name = process.ProcessName, Location = selectedProcessLocation })
                    {
                        Icon = IconHandler.ExtractIcon(selectedProcessLocation)
                    };
                    ProgramsInUI.Add(program);
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
                _mainViewModel.DatabaseJSON.AddProgramToDatabase(SelectedProcess.Model);
                _mainViewModel.StatusMessageService.Lower($"Program {SelectedProcess.Name} has been added to the Database!");
                _mainViewModel.UpdateProgram();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't add Windows protected process! {ex.Message}");
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
