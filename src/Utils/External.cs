using System.Runtime.InteropServices;

namespace B.Utils
{
    public static class External
    {
        private const string USER32 = "user32.dll";
        private const string KERNEL32 = "kernel32.dll";

        public static void Initialize()
        {
            // Copied Code to disable some window resizing functionality
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/1aa43c6c-71b9-42d4-aa00-60058a85f0eb/c-console-window-disable-resize?forum=csharpgeneral
            IntPtr handle = External.GetConsoleWindow();

            if (handle != IntPtr.Zero)
            {
                IntPtr sysMenu = External.GetSystemMenu(handle, false);

                foreach (SystemCommand sc in new SystemCommand[] {
                    SystemCommand.SC_SIZE,
                    SystemCommand.SC_MINIMIZE,
                    SystemCommand.SC_MAXIMIZE,
                    SystemCommand.SC_CLOSE,
                })
                    External.DeleteMenu(sysMenu, (int)sc, 0x00000000);
            }
        }

        public enum SystemCommand
        {
            SC_SIZE = 0xF000,
            SC_MINIMIZE = 0xF020,
            SC_MAXIMIZE = 0xF030,
            SC_CLOSE = 0xF060,
        }

        [DllImport(USER32)]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport(USER32)]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport(KERNEL32, ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();
    }
}
