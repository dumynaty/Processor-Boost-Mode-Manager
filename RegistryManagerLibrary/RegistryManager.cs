using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace RegistryManagerLibrary
{
    [SupportedOSPlatform("windows")]
    public static class RegistryManager
    {
        private static RegistryKey? rk;

        public static readonly byte[] TaskManagerEnableValue = { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public readonly static string ProgramsStartupPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        public readonly static string TaskManagerStartupPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

        private readonly static string PowerSchemesPath = @"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\";
        private readonly static string PROCESSOR_SUBGROUP_GUID = "54533251-82be-4824-96c1-47b60b740d00";
        private readonly static string PROCESSOR_BOOST_MODE_GUID = "be337238-0d82-4146-a960-4f3749d470c7";
        private static string ProcessorBoostModePath = "Unknown";
        public static string ActivePowerScheme = "Unknown";

        // Base Methods
        public static void PowerShellCommand(string command)
        {
            var processInfo = new ProcessStartInfo("powershell.exe")
            {
                Arguments = $"-NoProfile -NonInteractive -Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };
            using (var process = Process.Start(processInfo))
            {
                process?.WaitForExit();
            }
        }

        public static void RegistryCurrentUserSetValue(string registryPath, string keyName, object keyValue, RegistryValueKind rVK = RegistryValueKind.Unknown)
        {
            using (rk = Registry.CurrentUser.OpenSubKey(registryPath, true))
            {
                rk?.SetValue(keyName, keyValue, rVK);
            }
        }
        public static void RegistryCurrentUserDeleteValue(string registryPath, string name)
        {
            using (rk = Registry.CurrentUser.OpenSubKey(registryPath, true))
            {
                rk?.DeleteValue(name, throwOnMissingValue: false);
            }
        }
        public static object? RegistryCurrentUserGetValue(string registryPath, string name)
        {
            using (rk = Registry.CurrentUser.OpenSubKey(registryPath, false))
            {
                return rk?.GetValue(name);
            }
        }

        public static void RegistryLocalMachineSetValue(string registryPath, string keyName, object keyValue, RegistryValueKind rVK = RegistryValueKind.Unknown)
        {
            using (rk = Registry.LocalMachine.OpenSubKey(registryPath, true))
            {
                rk?.SetValue(keyName, keyValue, rVK);
            }
        }
        public static void RegistryLocalMachineDeleteValue(string registryPath, string name)
        {
            using (rk = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                rk?.DeleteValue(name, throwOnMissingValue: false);
            }
        }
        public static object? RegistryLocalMachineGetValue(string registryPath, string name)
        {
            using (rk = Registry.LocalMachine.OpenSubKey(registryPath, false))
            {
                return rk?.GetValue(name);
            }
        }

        // App related Methods
        public static void RegisterAppToStartup(string appName, string appPath)
        {
            try
            {
                RegistryCurrentUserSetValue(ProgramsStartupPath, appName, appPath);
                RegistryCurrentUserSetValue(TaskManagerStartupPath, appName, TaskManagerEnableValue, RegistryValueKind.Binary);
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't register app to startup! Please check permissions! {e.Message}");
            }
        }
        public static void UnregisterAppFromStartup(string appName)
        {
            try
            {
                RegistryCurrentUserDeleteValue(ProgramsStartupPath, appName);
                RegistryCurrentUserDeleteValue(TaskManagerStartupPath, appName);
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't unregister app from startup! Please check permissions! {e.Message}");
            }
        }
        public static bool IsAppStartupEnabled(string appName)
        {
            try
            {
                object? RegistryStartupApp = RegistryCurrentUserGetValue(ProgramsStartupPath, appName);
                byte[]? TMValue = RegistryCurrentUserGetValue(TaskManagerStartupPath, appName) as byte[];

                if (RegistryStartupApp != null && TMValue != null && TMValue.Length > 0 && TMValue[0] == 0x02)
                    return true;
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't read application startup registry keys. {e.Message}");
            }

            return false;
        }

        public static void GetActivePowerScheme()
        {
            try
            {
                var activePowerScheme = RegistryLocalMachineGetValue(PowerSchemesPath, "ActivePowerScheme");
                if (activePowerScheme != null)
                {
                    ActivePowerScheme = (string)activePowerScheme;
                }

                var activeOverlayAcPowerScheme = RegistryLocalMachineGetValue(PowerSchemesPath, "ActiveOverlayAcPowerScheme");
                if (activeOverlayAcPowerScheme != null && !activeOverlayAcPowerScheme.Equals("00000000-0000-0000-0000-000000000000"))
                {
                    ActivePowerScheme = (string)activeOverlayAcPowerScheme;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't get the active Power Scheme! {e.Message}");
            }
        }
        public static void ConfigureProcessorBoostModeForActivePowerScheme()
        {
            try
            {
                if (RegistryLocalMachineGetValue(ProcessorBoostModePath, "ACSettingIndex") == null)
                {
                    string commands = $@"
                        powercfg /setacvalueindex {ActivePowerScheme} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} 2;
                        powercfg /setdcvalueindex {ActivePowerScheme} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} 2;
                        powercfg /setactive SCHEME_CURRENT";

                    PowerShellCommand(commands);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error trying to prepare Processor Boost Mode values for the current GUID Plan. {e.Message}");
            }
        }
        public static int GetProcessorBoostMode()
        {
            try
            {
                ProcessorBoostModePath = PowerSchemesPath + ActivePowerScheme + "\\" + PROCESSOR_SUBGROUP_GUID + "\\" + PROCESSOR_BOOST_MODE_GUID;
                var value = RegistryLocalMachineGetValue(ProcessorBoostModePath, "ACSettingIndex");
                if (value != null)
                {
                    return (int)value;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Could not get Processot Boost Mode. Check app permissions! {e.Message}");
            }
            return -1;
        }
        public static void SetProcessorBoostMode(int boostMode)
        {
            int[] boostModeValues = [0,1,2,3,4,5,6];
            try
            {
                if (!string.IsNullOrEmpty(ActivePowerScheme) && boostModeValues.Contains(boostMode))
                {
                    string commands = $@"
                    powercfg /setacvalueindex {ActivePowerScheme} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} {boostMode};
                    powercfg /setdcvalueindex {ActivePowerScheme} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} 0;
                    powercfg /setactive SCHEME_CURRENT";

                    PowerShellCommand(commands);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error setting new processor boost mode! {e.Message}");
                
                // Add ConfigureProcessorBoostModeForActivePowerScheme() for that specific error.
            }
        }
    }
}
