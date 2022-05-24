using System.Runtime.InteropServices;
using B.Utils.Enums;
using B.Utils.Extensions;

namespace B.Utils
{
    public static class External
    {
        #region Win32 API

        // String representing 'User32.DLL' reference.
        private const string USER32 = "user32.dll";
        // String representing 'Kernel32.DLL' reference.
        private const string KERNEL32 = "kernel32.dll";

        #endregion



        #region Cache

        // Handle of the console window.
        private static IntPtr _consoleHandle;

        #endregion



        #region Universal Methods

        // Initializes console window properties using Win32 API.
        public static void Initialize()
        {
            DisableWindowResizing();
            DisableTextSelection();
        }

        // Get position of mouse cursor relative to the console window.
        public static Vector2 GetRelativeMousePosition()
        {
            GetCursorPos(out Point point);
            GetWindowRect(_consoleHandle, out Rect rect);
            int x = point.x - rect.x;
            int y = point.y - rect.y;
            return new Vector2(x, y);
        }

        // Get pressed state of left mouse button.
        public static bool GetLeftMouseButtonDown() => GetKeyStatePressed(VirtualKeyStates.VK_LBUTTON);

        #endregion



        #region Private Methods

        // Gets the state of specified key.
        private static bool GetKeyStatePressed(VirtualKeyStates key) => Convert.ToBoolean(GetKeyState(key) & 0x8000);

        // Disables the ability to resize the console window using banner buttons or edge dragging.
        private static void DisableWindowResizing()
        {
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/1aa43c6c-71b9-42d4-aa00-60058a85f0eb/c-console-window-disable-resize?forum=csharpgeneral

            _consoleHandle = GetConsoleWindow();

            if (_consoleHandle != IntPtr.Zero)
            {
                IntPtr sysMenu = GetSystemMenu(_consoleHandle, false);
                Enum.GetValues<SystemCommand>().ForEach(sc => DeleteMenu(sysMenu, (int)sc, 0x0000_0000));
            }
            else
                throw new Exception("Failed to get console window handle.");
        }

        // Disables the ability to select text in the console window.
        private static void DisableTextSelection()
        {
            // https://stackoverflow.com/a/36720802

            // -10 is the standard input device
            IntPtr handle = GetStdHandle(-10);

            if (!GetConsoleMode(handle, out uint consoleMode))
                throw new Exception("Failed to get console mode.");

            // 0b1000000 (quick edit mode)
            consoleMode &= ~(uint)(0b100_0000);

            if (!SetConsoleMode(handle, consoleMode))
                throw new Exception("Failed to set console mode.");
        }

        #endregion



        #region Local Utils

        // System Commands for disabling window resizing.
        private enum SystemCommand
        {
            SC_SIZE = 0xF000,
            SC_MINIMIZE = 0xF020,
            SC_MAXIMIZE = 0xF030,
            SC_CLOSE = 0xF060,
        }

        // Struct representing a point.
        // Used to get the position of the mouse cursor.
        private struct Point
        {
            public int x;
            public int y;
        }

        // Struct representing a rectangle.
        // Used to get the position of the console window.
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

        // External Win 32 API methods.
        // https://www.pinvoke.net/

        [DllImport(USER32)]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport(USER32)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport(USER32)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport(USER32)]
        private static extern bool GetCursorPos(out Point lpPoint);

        [DllImport(USER32)]
        private static extern short GetKeyState(VirtualKeyStates nVirtKey);

        [DllImport(KERNEL32)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(KERNEL32)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport(KERNEL32)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport(KERNEL32)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        #endregion
    }
}
