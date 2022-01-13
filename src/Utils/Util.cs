using System.Xml.Serialization;

namespace B.Utils
{
    public static class Util
    {
        public const char NULLCHAR = default(char);

        public static readonly Random Random = new Random();

        public static ConsoleKeyInfo LastInput { get; private set; }

        public static void WaitForInput() { Console.ReadKey(true); }

        public static ConsoleKeyInfo GetInput()
        {
            Util.LastInput = Console.ReadKey(true);
            return Util.LastInput;
        }

        public static void WaitForKey(ConsoleKey key, bool displayMessage = true, int offsetLeft = 0)
        {
            if (displayMessage)
                Util.Print(string.Format("Press {0} to continue...", key), offsetLeft, linesBefore: 1);

            while (true)
                if (Util.GetInput().Key == key)
                    break;
        }

        public static int Clamp(int value, int min, int max) { return Math.Min(Math.Max(value, min), max); }

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

            string messageStr = message == null ? string.Empty : message.ToString();
            messageStr = string.Format("{0," + (messageStr.Length + offsetLeft).ToString() + "}", messageStr);

            if (newLine)
                Console.WriteLine(messageStr);
            else
                Console.Write(messageStr);
        }

        public static void SetConsoleSize(int width, int height)
        {
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
            Console.SetWindowSize(width, height);
        }

        public static void Serialize<T>(string filePath, T objectToWrite)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Create))
                new XmlSerializer(typeof(T)).Serialize(fileStream, objectToWrite);
        }

        public static T Deserialize<T>(string filePath)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open))
                return (T)new XmlSerializer(typeof(T)).Deserialize(fileStream);
        }
    }
}
