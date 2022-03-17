using System.Runtime.InteropServices;
using B.Utils.Extensions;

namespace B.Utils
{
    public static class External
    {
        private const string USER32 = "user32.dll";
        private const string KERNEL32 = "kernel32.dll";

        public static void Initialize()
        {
            IntPtr currentHandle;

            // Copied Code to disable some window resizing functionality
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/1aa43c6c-71b9-42d4-aa00-60058a85f0eb/c-console-window-disable-resize?forum=csharpgeneral
            currentHandle = GetConsoleWindow();

            if (currentHandle != IntPtr.Zero)
            {
                IntPtr sysMenu = GetSystemMenu(currentHandle, false);
                Enum.GetValues<SystemCommand>().ForEach(sc => DeleteMenu(sysMenu, (int)sc, 0x00000000));
            }
            else
                throw new Exception("Failed to get console window handle.");

            // Copied Code to disable text selection functionality
            // https://stackoverflow.com/a/36720802

            // -10 is the standard input device
            currentHandle = GetStdHandle(-10);

            if (GetConsoleMode(currentHandle, out uint consoleMode))
            {
                // 0x0040 controls text selection (quick edit mode)
                consoleMode &= ~(uint)(0x0040);

                if (!SetConsoleMode(currentHandle, consoleMode))
                    throw new Exception("Failed to set console mode.");
            }
            else
                throw new Exception("Failed to get console mode.");
        }

        private enum SystemCommand
        {
            SC_SIZE = 0xF000,
            SC_MINIMIZE = 0xF020,
            SC_MAXIMIZE = 0xF030,
            SC_CLOSE = 0xF060,
        }

        [DllImport(USER32)]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport(USER32)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport(KERNEL32, ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(KERNEL32, SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport(KERNEL32)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport(KERNEL32)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    }
}
