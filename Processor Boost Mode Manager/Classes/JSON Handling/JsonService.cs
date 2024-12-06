using System.IO;
using System.Text.Json;


namespace ProcessBoostModeManager
{
    public class JsonService
    {
        public static string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Database.json");
        public static MainWindow? MainWindowInstance { get; set; }
        public static void AddToFile(ProgramModel program)
        {
            List<ProgramModel> programs;
            try
            {
                var jsonContent = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    programs = new List<ProgramModel>();
                }
                else
                {
                    programs = JsonSerializer.Deserialize<List<ProgramModel>>(jsonContent) ?? new List<ProgramModel>();
                }
            }
            catch (JsonException)
            {
                MessageBox.Show("Failed to add to Database due to JSON Exception! Reseting Database.json List..");
                programs = new List<ProgramModel>();
            }

            for (int i = 0; i < programs.Count; i++)
            {
                if (programs[i].Name == program.Name)
                {
                    TextBoxHandling.Lower($"Program '{program.Name}' is already in the Database!");
                    return;
                }  
            }
            programs.Add(program);
            SavePrograms(programs);

            // Update ProgramsInUI when adding a program
            if (MainWindowInstance != null)
            {
                program.Icon = Processes.ExtractIcon(program.Location);
                MainWindowInstance.ProgramsInUI.Add(program);
                TextBoxHandling.Lower($"Program '{program.Name}' has been added to the Database!");
            }
        }

        public static void ManualAdd()
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
                    Location = fileLocation,
                    BoostMode = "Disabled"
                };
                JsonService.AddToFile(program);
            }
        }
        public static void RemoveProcess(ProgramModel program)
        {
            List<ProgramModel> programs;
            var jsonContent = File.ReadAllText(FilePath);
            programs = JsonSerializer.Deserialize<List<ProgramModel>>(jsonContent) ?? new List<ProgramModel>();
            programs.RemoveAll(p => p.Name == program.Name);
            SavePrograms(programs);

            if (MainWindowInstance != null)
                MainWindowInstance.ProgramsInUI.Remove(program);
            TextBoxHandling.Lower($"Program '{program.Name}' has been removed!");
        }
        public static void SavePrograms(List<ProgramModel>? programs = null)
        {
            if (programs == null)
            {
                var jsonContent = File.ReadAllText(FilePath);
                programs = JsonSerializer.Deserialize<List<ProgramModel>>(File.ReadAllText(FilePath)) ?? new List<ProgramModel>();
            }
            File.WriteAllText(FilePath, JsonSerializer.Serialize(programs, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    }
}
