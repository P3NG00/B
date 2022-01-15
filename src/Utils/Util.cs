using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace B.Utils
{
    public static class Util
    {
        public const char NULL_CHAR = default(char);
        public const string NULL_STRING = "";
        public const int MAX_CHARS_DECIMAL = 27;

        public static readonly Random Random = new Random();

        public static ConsoleKeyInfo LastInput { get; private set; }

        public static void WaitForInput() => Console.ReadKey(true);

        public static ConsoleKeyInfo GetInput() => Util.LastInput = Console.ReadKey(true);

        public static void WaitForKey(ConsoleKey key, bool displayMessage = true, int offsetLeft = 0)
        {
            if (displayMessage)
                Util.Print($"Press {key} to continue...", offsetLeft, linesBefore: 1);

            while (true)
                if (Util.GetInput().Key == key)
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

        public static void Print(object? message = null, int offsetLeft = 0, bool newLine = true, int linesBefore = 0)
        {
            for (int i = 0; i < linesBefore; i++)
                Console.WriteLine();

            string messageStr = message?.ToString() ?? string.Empty;
            messageStr = string.Format("{0," + (messageStr.Length + offsetLeft).ToString() + "}", messageStr);

            if (newLine)
                Console.WriteLine(messageStr);
            else
                Console.Write(messageStr);
        }

        public static void SetConsoleSize(int width, int height)
        {
            // This can only be called on Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.SetWindowSize(width, height);
                Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
                // This is called twice to fix the scrollbar from showing
                Console.SetWindowSize(width, height);
            }
        }

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
