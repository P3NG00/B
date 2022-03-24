using Newtonsoft.Json;

namespace B.Utils
{
    public static class Data
    {
        private static readonly JsonSerializer _serializer = new();

        static Data() => _serializer.Formatting = Formatting.Indented;

        public static void Serialize(string filePath, object objectToWrite)
        {
            using (StreamWriter file = File.CreateText(filePath))
                _serializer.Serialize(file, objectToWrite);
        }

        public static T Deserialize<T>(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
                return (T)_serializer.Deserialize(file, typeof(T))!;
        }
    }
}
