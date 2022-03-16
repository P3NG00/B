using Newtonsoft.Json;

namespace B.Utils
{
    public static class Data
    {
        public static void Serialize(string filePath, object objectToWrite)
        {
            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, objectToWrite);
            }
        }

        public static T Deserialize<T>(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                T t = (T)serializer.Deserialize(file, typeof(T))!;
                return t;
            }
        }
    }
}
