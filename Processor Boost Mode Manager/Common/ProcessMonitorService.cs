using ProcessorBoostModeManager.Enums;
using ProcessorBoostModeManager.Models.Poco;
using ProcessorBoostModeManager.ViewModels;
using RegistryManagerLibrary;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace ProcessorBoostModeManager.Common
{
    public class ProcessMonitorService
    {
        readonly MainViewModel _model;
        readonly DispatcherTimer Timer;

        public ProcessMonitorService(MainViewModel model)
        {
            _model = model;
            Timer = new DispatcherTimer();
        }

        private (ObservableCollection<ProgramViewModel> Database, CPUBoostMode HighestBoostMode, int RunningProgramsCount) GetProcessedDatabase(List<ProgramModel> PocoDatabase)
        {
            var Database = new ObservableCollection<ProgramViewModel>();
            var WindowsProcesses = GetWindowsProcesses();
            CPUBoostMode HighestBoostMode = 0;
            int RunningProgramsCount = 0;

            foreach (var program in PocoDatabase)
            {
                ProgramViewModel fullProgram = new(program);
                if (WindowsProcesses.Contains(fullProgram.Name))
                {
                    fullProgram.IsRunning = true;
                    RunningProgramsCount++;

                    if ((int)fullProgram.BoostMode > (int)HighestBoostMode)
                        HighestBoostMode = fullProgram.BoostMode;
                }

                Database.Add(fullProgram);
            }

            foreach (var fullProgram in Database)
            {
                if (fullProgram.BoostMode == HighestBoostMode && fullProgram.BoostMode != 0 && fullProgram.IsRunning == true)
                {
                    fullProgram.HighestValue = true;
                }
            }

            return (Database, HighestBoostMode, RunningProgramsCount);
        }
        public void RunTimer(bool restart = false)
        {
            if (restart == true)
            {
                _model.ProcessMonitorService.UpdateProgram();
                Timer.Stop();
            }

            Timer.Interval = TimeSpan.FromSeconds(_model.SavedSettingsService.UpdateSpeed);
            Timer.Tick += (s, e) =>
            {
                UpdateProgram();
            };
            Timer.Start();
        }
        public void UpdateProgram()
        {
            var (Database, NewHighestBoostMode, NewRunningProgramsCount) = GetProcessedDatabase(_model.DatabaseService.PocoDatabase);
            bool RefreshUI = false;

            if (_model.ProgramsInUI.Count == 0)
            {
                // Do nothing if Database is empty
                if (Database.Count == 0)
                {
                    _model.DatabaseIsEmpty = true;
                    return;
                }

                // Populate ProgramsInUI
                foreach (var program in Database)
                {
                    program.Icon = IconHandler.ExtractIcon(program.Location);
                    if (program.Icon == null)
                    {
                        program.Icon = IconHandler.ApplyUnknownIcon();
                        program.ComboBoxSelection.ComboBoxVisibility = System.Windows.Visibility.Collapsed;
                    }
                    _model.ProgramsInUI.Add(program);
                }
                _model.AdjustComboBoxBoostModes();
                _model.DatabaseIsEmpty = false;
            }

            // Same programs, check for changes in IsRunning / HighestValue
            if (_model.ProgramsInUI.Count == Database.Count)
            {
                if (_model.RunningProgramsCount != NewRunningProgramsCount)
                    _model.RunningProgramsCount = NewRunningProgramsCount;

                for (int i = 0; i < Database.Count; i++)
                {
                    if (_model.ProgramsInUI[i].IsRunning != Database[i].IsRunning)
                    {
                        _model.ProgramsInUI[i].IsRunning = Database[i].IsRunning;
                        RefreshUI = true;
                    }
                    if (_model.ProgramsInUI[i].HighestValue != Database[i].HighestValue)
                    {
                        _model.ProgramsInUI[i].HighestValue = Database[i].HighestValue;
                        RefreshUI = true;
                    }
                }
            }
            // Add or Remove programs if needed
            else
            {
                string[] programsInUIArray = _model.ProgramsInUI.Select(Program => Program.Name).ToArray();
                string[] programsInDBArray = Database.Select(Program => Program.Name).ToArray();

                if (_model.ProgramsInUI.Count < Database.Count)
                {
                    foreach (var program in Database)
                    {
                        if (!programsInUIArray.Contains(program.Name))
                        {
                            program.Icon = IconHandler.ExtractIcon(program.Location);
                            if (program.Icon == null)
                            {
                                program.Icon = IconHandler.ApplyUnknownIcon();
                                program.ComboBoxSelection.ComboBoxVisibility = System.Windows.Visibility.Collapsed;
                            }

                            _model.ProgramsInUI.Add(program);
                            _model.AdjustComboBoxBoostModes();
                            break;
                        }
                    }
                }
                else if (_model.ProgramsInUI.Count > Database.Count)
                {
                    foreach (var program in _model.ProgramsInUI)
                    {
                        if (!programsInDBArray.Contains(program.Name))
                        {
                            _model.ProgramsInUI.Remove(program);
                            break;
                        }
                    }
                }
                RefreshUI = true;
            }

            if (RefreshUI == true)
                _model.ProgramsView.Refresh();

            ApplyBoostModeChange(NewHighestBoostMode);
        }

        private void ApplyBoostModeChange(CPUBoostMode NewHighestBoostMode)
        {
            // Check for difference
            int currentBoostMode = RegistryManager.GetProcessorBoostMode();
            if (currentBoostMode == (int)NewHighestBoostMode)
                return;

            // Apply change and update UI
            RegistryManager.SetProcessorBoostMode((int)NewHighestBoostMode);
            _model.CurrentBoostMode = NewHighestBoostMode;

            // Show Windows Notification if option is enabled
            if (_model.SavedSettingsService.WindowsNotification == true)
                App.trayIcon.ShowBalloonTip(2500, "Status change:", $"Current mode set to: {NewHighestBoostMode}", ToolTipIcon.None);
        }
        private List<string> GetWindowsProcesses()
        {
            return Process.GetProcesses().Select(p => p.ProcessName).Distinct().ToList();
        }
    }
}
