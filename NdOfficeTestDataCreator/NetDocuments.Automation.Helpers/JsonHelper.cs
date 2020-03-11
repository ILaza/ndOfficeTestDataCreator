using System;
using System.IO;

using Newtonsoft.Json;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Contains methods to work with JSON format.
    /// </summary>
    public class JsonHelper : IDisposable
    {
        private string dataStorageLocationPath;

        /// <summary>
        /// Creates new instance of JsonHelper.
        /// </summary>
        /// <param name="dataStorageLocationPath">Path to folder with data.</param>
        public JsonHelper(string dataStorageLocationPath)
        {
            this.dataStorageLocationPath = dataStorageLocationPath;
        }

        /// <summary>
        /// Fills object properties from .json data file.
        /// </summary>
        /// <typeparam name="T">Generic type of object to initialize.</typeparam>
        /// <param name="fileName">JSON file name.</param>
        /// <returns>Fully initialized object or null if any exception.</returns>
        public T Deserialize<T>(string fileName) where T : class
        {
            try
            {
                using (var streamReader = new StreamReader(Path.Combine(dataStorageLocationPath, fileName)))
                {
                    string json = streamReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch
            {
                // TODO: Log it using logger.
                // Will be implemented in scope of logger task.
                return null;
            }
        }

        /// <summary>
        /// Fills object properties from .json string.
        /// </summary>
        /// <typeparam name="T">Generic type of object to initialize.</typeparam>
        /// <param name="jsonString">JSON string.</param>
        /// <returns>Fully initialized object or null if any exception.</returns>
        public static T DeserializeString<T>(string jsonString) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch
            {
                // TODO: Log it using logger.
                // Will be implemented in scope of logger task.
                return null;
            }
        }

        /// <summary>
        /// Serializes a given object into string in JSON format.
        /// </summary>
        /// <param name="jsonObject">Object to serialize.</param>
        /// <returns>String in JSON format.</returns>
        public static string GetJsonString(object jsonObject)
        {
            if (jsonObject == null)
            {
                return null;
            }

            var formatting = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            return JsonConvert.SerializeObject(jsonObject, Formatting.Indented, formatting);
        }

        /// <summary>
        /// Implements disposing pattern.
        /// </summary>
        public void Dispose()
        {
            dataStorageLocationPath = null;
        }
    }
}