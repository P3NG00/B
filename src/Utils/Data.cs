using Newtonsoft.Json;

namespace B.Utils
{
    public static class Data
    {
        #region Private Variables

        // Serializer object for saving and loading data.
        private static readonly JsonSerializer _serializer = new()
        {
            Formatting = Formatting.Indented,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };

        #endregion



        #region Universal Methods

        // Serializes an object to a JSON file.
        public static void Serialize<T>(string filePath, T objectToWrite)
        {
            using (StreamWriter file = File.CreateText(filePath))
                _serializer.Serialize(file, objectToWrite);
        }

        // Deserializes a JSON file to an object.
        public static T Deserialize<T>(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
                return (T)_serializer.Deserialize(file, typeof(T))!;
        }

        #endregion
    }
}
