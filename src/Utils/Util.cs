using System.Xml;
using System.Xml.Serialization;

namespace B.Utils
{
    public static class Util
    {
        public static char[] FormatChars => new char[] { (char)7, (char)8, (char)9, (char)10, (char)13 };
        public static Random Random => new Random();

        public static ConsoleColor[] OrderedConsoleColors => new ConsoleColor[] {
            ConsoleColor.White,
            ConsoleColor.Gray,
            ConsoleColor.DarkGray,
            ConsoleColor.Black,
            ConsoleColor.DarkMagenta,
            ConsoleColor.Magenta,
            ConsoleColor.DarkBlue,
            ConsoleColor.Blue,
            ConsoleColor.DarkCyan,
            ConsoleColor.Cyan,
            ConsoleColor.DarkGreen,
            ConsoleColor.Green,
            ConsoleColor.DarkYellow,
            ConsoleColor.Yellow,
            ConsoleColor.DarkRed,
            ConsoleColor.Red
        };

        public static char Unformat(char c) => Util.FormatChars.Contains(c) ? ' ' : c;

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

        public static string Spaces(int spaces) => string.Empty.PadLeft(spaces);

        public static void ToggleBool(ref bool b) => b = !b;

        public static void Serialize<T>(string filePath, T objectToWrite)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Create))
            {
                XmlSerializer serializer = new(typeof(T));
                XmlSerializerNamespaces namespaces = new();
                namespaces.Add("", "");
                XmlWriterSettings settings = new();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;

                using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
                    serializer.Serialize(writer, objectToWrite, namespaces);
            }
        }

        public static T Deserialize<T>(string filePath)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open))
                return (T)new XmlSerializer(typeof(T)).Deserialize(fileStream)!;
        }
    }
}
