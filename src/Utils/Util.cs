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

        public static void Loop(int times, Action<int> action)
        {
            for (int i = 0; i < times; i++)
                action(i);
        }

        public static string StringOf(string str, int length)
        {
            string s = string.Empty;

            // TODO replace with Util.Loop
            for (int i = 0; i < length; i++)
                s += str;

            return s;
        }

        public static string StringOf(char c, int length) => Util.StringOf(c.ToString(), length);

        public static string Spaces(int spaces) => string.Empty.PadLeft(spaces);

        public static void Serialize<T>(string filePath, T objectToWrite)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Create))
            using (XmlWriter writer = XmlWriter.Create(fileStream, new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true }))
            {
                XmlSerializerNamespaces namespaces = new();
                namespaces.Add("", "");
                new XmlSerializer(typeof(T)).Serialize(writer, objectToWrite, namespaces);
            }
        }

        public static T Deserialize<T>(string filePath)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open))
                return (T)new XmlSerializer(typeof(T)).Deserialize(fileStream)!;
        }

        public static char Unformat(this char c) => Util.FormatChars.Contains(c) ? ' ' : c;
    }
}
