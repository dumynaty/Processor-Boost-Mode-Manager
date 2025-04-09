using ProcessorBoostModeManager.Models.Poco;
using ProcessorBoostModeManager.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ProcessorBoostModeManager.Common
{
    public class ProcessMonitorService
    {
        public (ObservableCollection<ProgramViewModel> Database, int HighestBoostMode, int RunningProgramsCount) GetProcessedDatabase(List<ProgramModel> PocoDatabase)
        {
            var Database = new ObservableCollection<ProgramViewModel>();
            var WindowsProcesses = GetWindowsProcesses();
            int HighestBoostMode = 0;
            int RunningProgramsCount = 0;

            foreach (var program in PocoDatabase)
            {
                ProgramViewModel fullProgram = new(program);
                if (WindowsProcesses.Contains(fullProgram.Name))
                {
                    fullProgram.IsRunning = true;
                    RunningProgramsCount++;

                    if ((int)fullProgram.BoostMode > HighestBoostMode)
                        HighestBoostMode = (int)fullProgram.BoostMode;
                }

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

            return (Database, HighestBoostMode, RunningProgramsCount);
        }

        public List<string> GetWindowsProcesses()
        {
            return Process.GetProcesses().Distinct().Select(p => p.ProcessName).ToList();
        }
    }
}
