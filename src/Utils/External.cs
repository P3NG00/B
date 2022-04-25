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

        // This is cached because it is used in GetRelativeMousePosition()
        private static IntPtr _consoleHandle;

        #endregion



        #region Universal Methods

        public static void Initialize()
        {
            DisableWindowResizing();
            DisableTextSelection();
            // DisableWindowTitleBar(); // TODO need to implement way of moving window when using this
        }

        public static Vector2 GetRelativeMousePosition()
        {
            GetCursorPos(out Point point);
            GetWindowRect(_consoleHandle, out Rect rect);
            int x = point.x - rect.x;
            int y = point.y - rect.y;
            return new Vector2(x, y);
        }

        public static bool GetLeftMouseButtonDown()
        {
            // private const int KEY_PRESSED = 0x8000;
            return Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_LBUTTON) & 0x8000);
        }

        #endregion



        #region Private Methods

        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/1aa43c6c-71b9-42d4-aa00-60058a85f0eb/c-console-window-disable-resize?forum=csharpgeneral
        private static void DisableWindowResizing()
        {
            _consoleHandle = GetConsoleWindow();

            if (_consoleHandle != IntPtr.Zero)
            {
                IntPtr sysMenu = GetSystemMenu(_consoleHandle, false);
                Enum.GetValues<SystemCommand>().ForEach(sc => DeleteMenu(sysMenu, (int)sc, 0x0000_0000));
            }
            else
                throw new Exception("Failed to get console window handle.");
        }

        // https://stackoverflow.com/a/36720802
        private static void DisableTextSelection()
        {
            // -10 is the standard input device
            IntPtr currentHandle = GetStdHandle(-10);

            if (!GetConsoleMode(currentHandle, out uint consoleMode))
                throw new Exception("Failed to get console mode.");

            // 0b1000000 (quick edit mode)
            consoleMode &= ~(uint)(0b100_0000);

            if (!SetConsoleMode(currentHandle, consoleMode))
                throw new Exception("Failed to set console mode.");
        }

        // https://stackoverflow.com/a/2014514
        private static void DisableWindowTitleBar()
        {
            int hWnd = FindWindow("ConsoleWindowClass", Program.Title);
            long style = GetWindowLong(hWnd, -16L);
            style &= ~(0b1100_0000_0000_0000_0000_0001L);
            SetWindowLong(hWnd, -16L, style);
            SetWindowPos(hWnd, 0L, 0L, 0L, 0L, 0L, 0x27L); // TODO try playing with 0x27L or -16L value to see if height can be adjusted
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

        [DllImport(USER32)]
        private static extern short GetKeyState(VirtualKeyStates nVirtKey);

        [DllImport(USER32, EntryPoint = "GetWindowLongA")]
        private static extern long GetWindowLong(long hWnd, long nIndex);

        [DllImport(USER32, EntryPoint = "SetWindowLongA")]
        private static extern long SetWindowLong(long hWnd, long nIndex, long dwNewLong);

        [DllImport(USER32)]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport(USER32)]
        private static extern bool SetWindowPos(int hWnd, long hWndInsertAfter, long x, long y, long cx, long cy, long wFlags);

        // TODO https://www.pinvoke.net/default.aspx/user32.SetClipboardData
        // TODO https://www.pinvoke.net/default.aspx/user32.GetClipboardData

        #endregion
    }
}
