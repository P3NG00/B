using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace B.Utils
{
    public static class Util
    {
        public const int MAX_CHARS_DECIMAL = 27;
        public const int MAX_CONSOLE_HEIGHT = 66;

        public static ConsoleKeyInfo LastInput { get; private set; } = default(ConsoleKeyInfo);

        public static Random Random => new Random();

        public static ConsoleKeyInfo GetKey() => Util.LastInput = Console.ReadKey(true);

        public static void WaitForKey(ConsoleKey key)
        {
            Util.PrintLine();
            Util.PrintLine($"Press {key} to continue...");

            while (true)
                if (Util.GetKey().Key == key)
                    break;
        }

        public static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);

        public static string StringOf(string str, int length)
        {
            string s = string.Empty;

            for (int i = 0; i < length; i++)
                s += str;

            return s;
        }

        public static string StringOf(char c, int length) => Util.StringOf(c.ToString(), length);

        public static T RandomFrom<T>(params T[] list) => list[Util.Random.Next(list.Length)];

        public static void Print(object message, ConsoleColor? colorText = null, ConsoleColor? colorBackground = null)
        {
            // Cache old color values
            ConsoleColor? oldColorText = null;
            ConsoleColor? oldColorBackground = null;

            // Override colors if specified
            if (colorText.HasValue)
            {
                oldColorText = Console.ForegroundColor;
                Console.ForegroundColor = colorText.Value;
            }

            if (colorBackground.HasValue)
            {
                oldColorBackground = Console.BackgroundColor;
                Console.BackgroundColor = colorBackground.Value;
            }

            // Print message
            Console.Write(message);

            // Restore old color values if overriden
            if (oldColorText.HasValue)
                Console.ForegroundColor = oldColorText.Value;

            if (oldColorBackground.HasValue)
                Console.BackgroundColor = oldColorBackground.Value;
        }

        public static void PrintLine(object message = null!, ConsoleColor? colorText = null, ConsoleColor? colorBackground = null)
        {
            if (message != null)
                Util.Print(message, colorText, colorBackground);

            Console.WriteLine();
        }

        public static void PrintLines(int lines)
        {
            for (int i = 0; i < lines; i++)
                Console.WriteLine();
        }

        public static void PrintSpaces(int spaces) => Console.Write(string.Empty.PadLeft(spaces));

        public static void SetConsoleSize(int width, int height)
        {
            // This can only be called on Windows // TODO shouldn't matter, but needs testing on other types of consoles
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.SetWindowSize(width, height);
                Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
                // This is called twice to fix the scrollbar from showing
                Console.SetWindowSize(width, height);
            }
        }

        public static void SetConsoleSize(Vector2 size) => Util.SetConsoleSize(size.x, size.y);

        public static void ClearConsole(int width = 0, int height = 0)
        {
            Console.Clear();

            if (width != 0 && height != 0)
                Util.SetConsoleSize(width, height);
        }

        public static void ClearConsole(Vector2 size) => Util.ClearConsole(size.x, size.y);

        public static void ResetTextCursor() => Console.SetCursorPosition(0, 0);

        public static void ToggleBool(ref bool b) => b = !b;

        public static void Serialize<T>(string filePath, T objectToWrite)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Create))
                new XmlSerializer(typeof(T)).Serialize(fileStream, objectToWrite);
        }

        public static T Deserialize<T>(string filePath)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open))
                return (T)new XmlSerializer(typeof(T)).Deserialize(fileStream)!;
        }
    }
}
