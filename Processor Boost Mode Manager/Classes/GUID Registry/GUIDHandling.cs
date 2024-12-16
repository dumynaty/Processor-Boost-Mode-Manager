using Microsoft.Win32;
using System.Diagnostics;

namespace ProcessBoostModeManager
{
    public class GUIDHandling
    {
        private readonly static string PowerPlanLocation = @"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\";
        private readonly static string PROCESSOR_SUBGROUP_GUID = "54533251-82be-4824-96c1-47b60b740d00";
        private readonly static string PROCESSOR_BOOST_MODE_GUID = "be337238-0d82-4146-a960-4f3749d470c7";
        private static string? CurrentGUID;
        static RegistryKey? rk = null;

        public static void GetCurrentGUID()
        {
            using (rk = Registry.LocalMachine.OpenSubKey(PowerPlanLocation, false))
            {
                if (rk != null)
                {
                    var value = rk.GetValue("ActivePowerScheme");
                    if (value != null)
                    {
                        CurrentGUID = value.ToString();
                    }
                    else
                    {
                        MessageBox.Show("ActivePowerScheme value not found.");
                    }

                    value = rk.GetValue("ActiveOverlayAcPowerScheme");
                    if (value != null)
                    {
                        if (!value.Equals("00000000-0000-0000-0000-000000000000"))
                            CurrentGUID = value.ToString();
                    }
                    else
                    {
                        MessageBox.Show("ActiveOverlayAcPowerScheme value not found.!");
                    }
                }
                else
                {
                    MessageBox.Show("Registry key not found.");
                }
            }
        }
        public static void SetGUID()
        {
            int boostMode = CheckIfSameCurrentProcessorValue();
            if (boostMode == -1)
                { return; }

            try
            {
                if (string.IsNullOrEmpty(CurrentGUID))
                {
                    throw new Exception("Failed to retrieve current power scheme GUID");
                }
                
                string commands = $@"
                powercfg /setacvalueindex {CurrentGUID} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} {boostMode};
                powercfg /setdcvalueindex {CurrentGUID} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} 0;
                powercfg /setactive SCHEME_CURRENT";

                var processInfo = new ProcessStartInfo("powershell.exe")
                {
                    Arguments = $"-NoProfile -NonInteractive -Command \"{commands}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using var process = Process.Start(processInfo);
                process?.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying boost mode: {ex.Message}");
            }
        }

        public static int CheckIfSameCurrentProcessorValue()
        {
            string GUIDProcessorBoostModePath = PowerPlanLocation + CurrentGUID +"\\" + PROCESSOR_SUBGROUP_GUID + "\\" + PROCESSOR_BOOST_MODE_GUID;
            int boostMode = stringToIntBoostModeValue(Processes.highestBoostModeValue);

            using (rk = Registry.LocalMachine.OpenSubKey(GUIDProcessorBoostModePath, false))
            {
                if (rk != null)
                {
                    var value = rk.GetValue("ACSettingIndex");
                    if (value != null)
                    {
                        if ((int)value == boostMode)
                        {
                            return -1;
                        }
                        else
                        {
                            return boostMode;
                        }
                    }
                    MessageBox.Show("ACSettingIndex rk value null in GCPV()!");
                    return -1;
                }
                MessageBox.Show("Could not access current boostMode! Check app permissions!");
                return -1;
            }
        }

        public static string GetWindowsProcessorBoostMode()
        {
            string GUIDProcessorBoostModePath = PowerPlanLocation + CurrentGUID + "\\" + PROCESSOR_SUBGROUP_GUID + "\\" + PROCESSOR_BOOST_MODE_GUID;
            int highestValueAsInt = -1;

            using (rk = Registry.LocalMachine.OpenSubKey(GUIDProcessorBoostModePath, false))
            {
                if (rk != null)
                {
                    var value = rk.GetValue("ACSettingIndex");
                    if (value != null)
                    {
                        highestValueAsInt = (int)value;
                    }
                    else
                    {
                        MessageBox.Show("ACSettingIndex rk value null in GCPBM()!");
                    }
                }
                else
                    MessageBox.Show("Could not access current processes boost mode! Check app permissions!");
            }

            string highestValue = intToStringBoostModeValue(highestValueAsInt);
            return highestValue;
        }

        public static int stringToIntBoostModeValue(string highestValueAsString)
        {
                int highestValueAsInt = highestValueAsString switch
                {
                    "Disabled" => 0,
                    "Enabled" => 1,
                    "Aggressive" => 2,
                    "Efficient Enabled" => 3,
                    "Efficient Aggressive" => 4,
                    "Aggressive At Guaranteed" => 5,
                    "Efficient Aggressive At Guaranteed" => 6, // Values 3, 4, 5, 6 are not currently implemented into the ComboBox.
                    _ => 0
                };
            return highestValueAsInt;
        }
        public static string intToStringBoostModeValue(int highestValueAsInt)
        {
            string highestValueAsString = highestValueAsInt switch
            {
                0 => "Disabled",
                1 => "Enabled",
                2 => "Aggressive",
                3 => "Efficient Enabled",
                4 => "Efficient Aggressive",
                5 => "Aggressive At Guaranteed",
                6 => "Efficient Aggressive At Guaranteed",
                _ => "Unknown",
            };
            return highestValueAsString;
        }

        public static void InitialSetup()
        {
            string GUIDProcessorBoostModePath = PowerPlanLocation + CurrentGUID + "\\" + PROCESSOR_SUBGROUP_GUID + "\\" + PROCESSOR_BOOST_MODE_GUID;

            using (rk = Registry.LocalMachine.OpenSubKey(GUIDProcessorBoostModePath, false))
            {
                if (rk == null)
                {
                    string commands = $@"
                    powercfg /setacvalueindex {CurrentGUID} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} 2;
                    powercfg /setdcvalueindex {CurrentGUID} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} 2;
                    powercfg /setactive SCHEME_CURRENT";

                    var processInfo = new ProcessStartInfo("powershell.exe")
                    {
                        Arguments = $"-NoProfile -NonInteractive -Command \"{commands}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    };
                    using var process = Process.Start(processInfo);
                    process?.WaitForExit();

                    MessageBox.Show("Path to GUID Key created and default values set!");
                }
            }
        }
    }
}
