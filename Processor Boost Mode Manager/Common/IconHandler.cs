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
                using var extractedIcon = System.Drawing.Icon.ExtractAssociatedIcon(programLocation);
                if (extractedIcon != null)
                {
                    var icon = Imaging.CreateBitmapSourceFromHIcon(
                        extractedIcon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    return icon;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error getting image from process (system protected / path unknown) - {e.Message}");
            }
            // !!! -- N2W -- !!!   -   Apply a standard unknown Icon
            return null;
        }
    }
}
