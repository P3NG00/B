using System.Runtime.InteropServices;
using B.Utils.Extensions;

namespace B.Utils
{
    public static class External
    {
        #region Win32 API

        private const string USER32 = "user32.dll";
        private const string KERNEL32 = "kernel32.dll";

        #endregion



        #region Cache

        private static IntPtr _consoleHandle;

        #endregion



        #region Universal Methods

        public static void Initialize()
        {
            // Disable Window Resizing Functionality
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/1aa43c6c-71b9-42d4-aa00-60058a85f0eb/c-console-window-disable-resize?forum=csharpgeneral
            _consoleHandle = GetConsoleWindow();

            if (_consoleHandle != IntPtr.Zero)
            {
                IntPtr sysMenu = GetSystemMenu(_consoleHandle, false);
                Enum.GetValues<SystemCommand>().ForEach(sc => DeleteMenu(sysMenu, (int)sc, 0x0000_0000));
            }
            else
                throw new Exception("Failed to get console window handle.");

            // Disable Text Selection Functionality
            // https://stackoverflow.com/a/36720802

            // -10 is the standard input device
            IntPtr currentHandle = GetStdHandle(-10);

            if (GetConsoleMode(currentHandle, out uint consoleMode))
            {
                // 0b1000000 (quick edit mode)
                consoleMode &= ~(uint)(0b100_0000);

                bool successful = SetConsoleMode(currentHandle, consoleMode);

                if (!successful)
                    throw new Exception("Failed to set console mode.");
            }
            else
                throw new Exception("Failed to get console mode.");
        }

        public static Vector2 GetRelativeMousePosition()
        {
            GetCursorPos(out Point point);
            GetWindowRect(_consoleHandle, out Rect rect);
            int x = point.x - rect.x;
            int y = point.y - rect.y;
            return new Vector2(x, y);
        }

        #endregion



        #region Local Utils

        private enum SystemCommand
        {
            SC_SIZE = 0xF000,
            SC_MINIMIZE = 0xF020,
            SC_MAXIMIZE = 0xF030,
            SC_CLOSE = 0xF060,
        }

        private struct Point
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int x => Left;
            public int y => Top;
        }

        #endregion



        #region External Methods

        [DllImport(USER32)]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport(USER32)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport(KERNEL32, ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(KERNEL32)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport(KERNEL32)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport(KERNEL32)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport(USER32)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport(USER32)]
        private static extern bool GetCursorPos(out Point lpPoint);

        #endregion
    }
}
