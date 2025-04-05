using ProcessorBoostModeManager.Models;
using ProcessorBoostModeManager.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ProcessorBoostModeManager.Common
{
    public class ProcessMonitorService
    {
        public (ObservableCollection<ProgramViewModel> Database, int HighestBoostMode) GetDatabaseProcessesOC(List<ProgramModel> programsFromDatabase)
        {
            var Database = new ObservableCollection<ProgramViewModel>();
            var WindowsProcesses = GetWindowsProcesses();

            int HighestBoostMode = 0;

            foreach (var program in programsFromDatabase)
            {
                ProgramViewModel fullProgram = new(program);
                if (WindowsProcesses.Contains(fullProgram.Name))
                {
                    fullProgram.IsRunning = true;

                    if ((int)fullProgram.BoostMode > HighestBoostMode)
                        HighestBoostMode = (int)fullProgram.BoostMode;
                }

                fullProgram.Icon = IconHandler.ExtractIcon(program.Location);

                Database.Add(fullProgram);
            }

            foreach (var fullProgram in Database)
            {
                if ((int)fullProgram.BoostMode == HighestBoostMode &&
                    (int)fullProgram.BoostMode != 0 &&
                    fullProgram.IsRunning == true)
                {
                    fullProgram.HighestValue = true;
                }
            }

            return (Database, HighestBoostMode);
        }

        public List<string> GetWindowsProcesses()
        {
            return Process.GetProcesses().Distinct().Select(p => p.ProcessName).ToList();
        }
    }
}
