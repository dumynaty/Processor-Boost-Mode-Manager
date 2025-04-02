using ProcessorBoostModeManager.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ProcessorBoostModeManager.Common
{
    public static class Processes
    {
        public static string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.json");

        public static void CreateDatabase()
        {
            try
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(new List<ProgramModel>(), new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
            }
            catch (Exception e)
            {
                throw new Exception($"Error creating Database.json {e.Message}");
            }
        }

        public static List<ProgramModel> GetDatabaseProcesses()
        {
            List<ProgramModel> ProgramsInDatabase = new List<ProgramModel>();
            try
            {
                var jsonContent = File.ReadAllText(FilePath);
                ProgramsInDatabase = JsonSerializer.Deserialize<List<ProgramModel>>(jsonContent) ?? new List<ProgramModel>();
            }
            catch (Exception e)
            {
                try
                {
                    System.Windows.MessageBox.Show("Creating Database...", "Database missing or corrupt", MessageBoxButton.OK, MessageBoxImage.Information);
                    CreateDatabase();
                }
                catch (Exception)
                {
                    throw new Exception($"Error reading Database file. {e.Message}");
                }
            }
            return ProgramsInDatabase;
        }

        public static void AddProgramToDatabase(ProgramModel newProgram)
        {
            var Database = GetDatabaseProcesses();

            if (!Database.Any(p => p.Name == newProgram.Name))
            {
                Database.Add(newProgram);
                SaveDatabase(Database);
            }
        }

        public static void RemoveProgramFromDatabase(ProgramModel program)
        {
            var Database = GetDatabaseProcesses();
            Database.RemoveAll(p => p.Name == program.Name);
            SaveDatabase(Database);
        }

        public static void SaveDatabase(List<ProgramModel> programs)
        {
            try
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(programs, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true,
                }));
            }
            catch (Exception e)
            {
                throw new Exception($"Error saving Database {e.Message}");
            }
        }

        public static List<string> GetWindowsProcesses()
        {
            return Process.GetProcesses().Distinct().Select(p => p.ProcessName).ToList();
        }

        public static (ObservableCollection<ProgramModel> Database, int HighestBoostMode) GetDatabaseProcessesOC()
        {
            var Database = new ObservableCollection<ProgramModel>();
            var WindowsProcesses = GetWindowsProcesses();

            int HighestBoostMode = 0;

            foreach (var program in GetDatabaseProcesses())
            {
                if (WindowsProcesses.Contains(program.Name))
                {
                    program.IsRunning = true;

                    if ((int)program.BoostMode > HighestBoostMode)
                        HighestBoostMode = (int)program.BoostMode;
                }

                program.Icon = IconHandler.ExtractIcon(program.Location);

                Database.Add(program);
            }

            foreach (var program in Database)
            {
                if ((int)program.BoostMode == HighestBoostMode &&
                    (int)program.BoostMode != 0 &&
                    program.IsRunning == true)
                {
                        program.HighestValue = true;
                }
            }

            return (Database, HighestBoostMode);
        }
    }
}
