// N2L
// https://stackoverflow.com/questions/334630/opening-a-folder-in-explorer-and-selecting-a-file

using System.Runtime.InteropServices;

namespace ProcessorBoostModeManager.Common.shell32
{
    public static class FileExplorer
    {
        [DllImport("shell32.dll")]
        private static extern int SHParseDisplayName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszName,
            IntPtr pbc,
            out IntPtr ppidl,
            uint sfgaoIn,
            out uint psfgaoOut);

        [DllImport("shell32.dll")]
        private static extern void SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[]? apidl, uint dwFlags);

        public static void ShowFileInExplorer(string filePath)
        {
            IntPtr pidl;
            uint sfgaoOut;
            if (SHParseDisplayName(filePath, IntPtr.Zero, out pidl, 0, out sfgaoOut) == 0)
            {
                try
                {
                    SHOpenFolderAndSelectItems(pidl, 0, null, 0);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pidl);
                }
            }
        }
    }
}
