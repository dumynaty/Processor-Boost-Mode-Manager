using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace ProcessBoostModeManager
{
    public class Processes
    {
        public static string highestBoostModeValue = "Disabled";
        public static int runningProcessesCount = 0;

        public static List<ProgramModel> GetListOfProgramsFromDatabase()
        {
            List<ProgramModel> programs = new();
            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.json");
            if (!File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, "[]");
            }
            else
            {
                try
                {
                    var jsonData = File.ReadAllText(FilePath);
                    programs = JsonSerializer.Deserialize<List<ProgramModel>>(jsonData) ?? new();
                }
                catch (JsonException)
                {
                    MessageBox.Show("Setting Database.json as Valid..");
                    programs = new List<ProgramModel>();
                    JsonService.SavePrograms(programs);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"P.GLOPFD() - File error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"P.GLOPFD() - Unexpected error: {ex.Message}");
                }
            }
            return programs;
        }
        public static List<ProgramModel> SetRunningProcesses(List<ProgramModel> programs)
        {
            runningProcessesCount = 0;
            var runningProcesses = Process.GetProcesses().Distinct().Select(p => p.ProcessName).ToList();

            foreach (var program in programs)
            {
                if (runningProcesses.Contains(program.Name))
                {
                    program.IsRunning = true;
                    runningProcessesCount++;
                }
            }
            return programs;
        }
        public static List<ProgramModel> SetHighestBoostMode(List<ProgramModel> programs)
        {
            highestBoostModeValue = "Disabled"; // Needs to be defined otherwise when all programs are changed to Disabled it will not refresh

            foreach (var program in programs)
            {
                if (program.IsRunning == true && program.BoostMode == "Enabled" && highestBoostModeValue == "Disabled")
                {
                    highestBoostModeValue = "Enabled";
                }
                else if (program.IsRunning == true && program.BoostMode == "Aggressive" && highestBoostModeValue != "Aggressive")
                {
                    highestBoostModeValue = "Aggressive";
                }
            }
            return programs;
        }
        public static List<ProgramModel> SetHighestValue(List<ProgramModel> programs)
        {
            foreach (var program in programs)
            {
                
                if (program.IsRunning == true && program.BoostMode == highestBoostModeValue && highestBoostModeValue != "Disabled")
                    program.HighestValue = true;
                else if (program.BoostMode != highestBoostModeValue)
                    program.HighestValue = false;
            }
            return programs;
        }
        public static List<ProgramModel> SetProgramIcon(List<ProgramModel> programs)
        {
            foreach (var program in programs)
            {
                program.Icon = ExtractIcon(program.Location);
            }
            return programs;
        }
        public static List<ProgramModel> GetUpdatedListOfPrograms()
        {
            var ProgramsInDatabase = GetListOfProgramsFromDatabase();
            ProgramsInDatabase = SetRunningProcesses(ProgramsInDatabase);
            ProgramsInDatabase = SetHighestBoostMode(ProgramsInDatabase);
            ProgramsInDatabase = SetHighestValue(ProgramsInDatabase);
            ProgramsInDatabase = SetProgramIcon(ProgramsInDatabase);

            if (ProgramsInDatabase.Count == 0)
            {
                TextBoxHandling.Upper("Add programs to the list!");
                TextBoxHandling.Lower("No programs loaded. Database empty!");
            }
            else
            {
                TextBoxHandling.Upper("Current processes running: " + runningProcessesCount, true, true);
            }
            return ProgramsInDatabase;
        }

        // Add Dictionary cache for when the list is Updated
        private readonly static Dictionary<string, BitmapSource> iconCache = new();
        public static BitmapSource? ExtractIcon(string programLocation)
        {
            if (iconCache.TryGetValue(programLocation, out var cachedIcon))
            {
                return cachedIcon;
            }
            try
            {
                using var extractedIcon = Icon.ExtractAssociatedIcon(programLocation);
                if (extractedIcon != null)
                {
                    var icon = Imaging.CreateBitmapSourceFromHIcon(
                        extractedIcon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    iconCache[programLocation] = icon;
                    return icon;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error getting image from process (system protected / path 'Unknown')");
            }
            return null;
        }
    }
}
