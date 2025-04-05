using ProcessorBoostModeManager.Models;
using ProcessorBoostModeManager.ViewModels;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using System.Windows;

namespace ProcessorBoostModeManager.Common
{
    public class DatabaseService
    {
        public string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.json");

        public void CreateDatabase()
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

        public List<ProgramModel> GetDatabaseProcesses()
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

        public void AddProgramToDatabase(ProgramModel newProgram)
        {
            var Database = GetDatabaseProcesses();

            if (!Database.Any(p => p.Name == newProgram.Name))
            {
                Database.Add(newProgram);
                SaveDatabase(Database);
            }
        }

        public void RemoveProgramFromDatabase(ProgramModel program)
        {
            var Database = GetDatabaseProcesses();
            Database.RemoveAll(p => p.Name == program.Name);
            SaveDatabase(Database);
        }

        public void SaveDatabase(List<ProgramModel> programs)
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
        public void SaveDatabase(List<ProgramViewModel> programsAsList)
        {
            List<ProgramModel> programs = new List<ProgramModel>();
            foreach (var program in programsAsList)
            {
                programs.Add(program.Model);
            }

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
    }
}
