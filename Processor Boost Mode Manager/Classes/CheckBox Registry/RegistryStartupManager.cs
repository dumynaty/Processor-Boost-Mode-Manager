using Microsoft.Win32;
using System.Reflection;

namespace ProcessorBoostModeManager
{
    public class RegistryStartupManager
    {
        private readonly string appName;
        private readonly string appPath;
        private readonly string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private readonly string registryPathApprovedTM = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

        public RegistryStartupManager()
        {
            appName = Assembly.GetExecutingAssembly().GetName().Name ?? "Processor Boost Mode Application Setting";
            appPath = Environment.ProcessPath ?? string.Empty;
        }

        public void RegisterStartup()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryPath, true); // true - Read-Write, false - Read-Only
                using RegistryKey? keyApprovedTM = Registry.CurrentUser.OpenSubKey(registryPathApprovedTM, true);

                if (key == null) // if Path does not exist / not given acess (not to be confused with value)
                {
                    MessageBox.Show("Failed to access registry key (@\"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run). Ensure proper permissions!");
                    return;
                }
                if (keyApprovedTM == null)
                {
                    MessageBox.Show("Failed to access registry key (@SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run\\-APPNAME-). Ensure proper permissions!");
                    return;
                }

                byte[] enableValue = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                key.SetValue(appName, appPath);
                keyApprovedTM.SetValue(appName, enableValue, RegistryValueKind.Binary);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void UnregisterStartup()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryPath, true);
                using RegistryKey? keyApprovedTM = Registry.CurrentUser.OpenSubKey(registryPathApprovedTM, true);
                if (key == null || keyApprovedTM == null)
                {
                    MessageBox.Show("Failed to access the registry key. Ensure proper permissions!");
                    return;
                }
                key.DeleteValue(appName, throwOnMissingValue: false);
                keyApprovedTM.DeleteValue(appName, throwOnMissingValue: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool IsAutostartEnabled()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryPath, false);
                using RegistryKey? keyApprovedTM = Registry.CurrentUser.OpenSubKey(registryPathApprovedTM, false);

                if (key == null || keyApprovedTM == null)
                    return false;

                byte[]? value = keyApprovedTM.GetValue(appName) as byte[];
                if (value != null && value.Length > 0 && value[0] == 0x02 && key.GetValue(appName) != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

    }
}
