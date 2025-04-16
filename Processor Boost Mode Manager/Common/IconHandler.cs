using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;

namespace ProcessorBoostModeManager.Common
{
    public class IconHandler
    {
        public static BitmapSource? ExtractIcon(string programLocation)
        {
            try
            {
                using var extractedIcon = Icon.ExtractAssociatedIcon(programLocation);
                if (extractedIcon != null)
                {
                    var icon = Imaging.CreateBitmapSourceFromHIcon(
                        extractedIcon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    return icon;
                }
            }
            catch (Exception)
            {
                //throw new Exception($"Error getting image from process (system protected / path unknown) - {e.Message}");
            }
            return null;
        }

        public static BitmapSource? ApplyUnknownIcon()
        {
            using var extractedIcon = Icon.ExtractAssociatedIcon("C:\\Windows\\HelpPane.exe");
            if (extractedIcon != null)
            {
                var icon = Imaging.CreateBitmapSourceFromHIcon(
                    extractedIcon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                return icon;
            }
            return null;
        }
    }
}
