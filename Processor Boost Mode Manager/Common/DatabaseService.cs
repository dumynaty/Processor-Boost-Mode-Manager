using ProcessorBoostModeManager.Models.Poco;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ProcessorBoostModeManager.Common
{
    public class DatabaseService
    {
        public List<ProgramModel> PocoDatabase;
        public DatabaseService()
        {
            PocoDatabase = GetDatabasePrograms();
        }
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
        public List<ProgramModel> GetDatabasePrograms()
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
            if (!PocoDatabase.Any(p => p.Name == newProgram.Name))
            {
                PocoDatabase.Add(newProgram);
                SaveDatabase(PocoDatabase);
            }
        }
        public void RemoveProgramFromDatabase(ProgramModel program)
        {
            PocoDatabase.RemoveAll(p => p.Name == program.Name);
            SaveDatabase(PocoDatabase);
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
            PocoDatabase = GetDatabasePrograms();
        }
    }
}
