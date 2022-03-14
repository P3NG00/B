using System.Xml;
using System.Xml.Serialization;

namespace B.Utils
{
    public static class Util
    {
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
    }
}
