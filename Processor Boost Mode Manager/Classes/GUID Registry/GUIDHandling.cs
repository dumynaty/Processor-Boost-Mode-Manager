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
        static RegistryKey? rk;

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
                }
                else
                {
                    MessageBox.Show("Registry key not found.");
                }
            }
        }

        public static void SetGUID()
        {
            int boostMode = GetCurrentProcessorValue();
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
                powercfg /setactive SCHEME_CURRENT
            ";

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

        public static int GetCurrentProcessorValue()
        {
            string valueCheck = PowerPlanLocation + CurrentGUID +"\\" + PROCESSOR_SUBGROUP_GUID + "\\" + PROCESSOR_BOOST_MODE_GUID;

            string highestValue = Processes.highestBoostModeValue;
            int boostModeMethod(string highestValue) =>
                highestValue switch
                {
                    "Disabled" => 0,
                    "Enabled" => 1,
                    "Aggressive" => 2,
                    _ => 0
                };
            int boostMode = boostModeMethod(highestValue);

            using (rk = Registry.LocalMachine.OpenSubKey(valueCheck, false))
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
                    MessageBox.Show("ACSettingIndex rk value null!");
                    return -1;
                }
                MessageBox.Show("Could not access current boostMode! Check app permissions!");
                return -1;
            }
        }

        public static string GetCurrentProcessorBoostMode()
        {
            string valueCheck = PowerPlanLocation + CurrentGUID + "\\" + PROCESSOR_SUBGROUP_GUID + "\\" + PROCESSOR_BOOST_MODE_GUID;
            int highestValueAsInt = -1;

            using (rk = Registry.LocalMachine.OpenSubKey(valueCheck, false))
            {
                if (rk != null)
                {
                    var value = rk.GetValue("ACSettingIndex");
                    if (value != null)
                    {
                        highestValueAsInt = (int)value;
                    }
                    else
                        MessageBox.Show("ACSettingIndex rk value null!");
                }
                else
                    MessageBox.Show("Could not access current processes boost mode! Check app permissions!");
            }

            string highestValue = highestValueAsInt switch
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
            return highestValue;
        }
    }
}
