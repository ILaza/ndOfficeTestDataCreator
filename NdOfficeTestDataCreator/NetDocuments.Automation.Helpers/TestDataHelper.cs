using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using NetDocuments.Client.COM.Models;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Holds methods to help to operate with test data.
    /// </summary>
    public static class TestDataHelper
    {
        private static string FoldersContainerName = "Folder Folders";

        private static List<string> SpecialPathItemsParts = new List<string>
        {
            "Folder",
            "Cabinet",
            "Repository"
        };

        /// <summary>
        /// Tries to get info with given key from given collection.
        /// </summary>
        /// <typeparam name="K">Collection key type.</typeparam>
        /// <typeparam name="T">Collection items type.</typeparam>
        /// <param name="collection">Collection instance for obtaining item from.</param>
        /// <param name="key">Item key.</param>
        /// <returns>Item if it exists in collection, otherwise null.</returns>
        public static T TryGetValueFromDictionary<K, T>(Dictionary<K, T> collection, K key) where T : class
        {
            var value = default(T);
            collection?.TryGetValue(key, out value);

            var clonedValue = (T)(value as ICloneable)?.Clone();
            if (clonedValue != null)
            {
                return clonedValue;
            }
            return  value;
        }

        /// <summary>
        /// Converts ProfileAttribute array object to ProfileAttributesCollection.
        /// </summary>
        /// <param name="attributes">ProfileAttribute array object to convert.</param>
        /// <returns>List of ProfileAttribute from array ProfileAttribute objects.</returns>
        public static List<ProfileAttribute> ToProfileAttributesList(ProfileAttribute[] attributes, int dateAttributeId)
        {
            if (attributes?.Any() == true)
            {
                var collection = new List<ProfileAttribute>();
                foreach (var item in attributes)
                {
                    // The Date attribute should be passed as DateTime object.
                    if (item.Id == dateAttributeId)
                    {
                        DateTime date;

                        // if item.Value is valid DateTime string, put it as DateTime object.
                        // Otherwise, put item.Value as is.
                        if (DateTime.TryParse(item.Value.ToString(), out date))
                        {
                            item.Value = date.ToShortDateString();
                        }
                    }
                    collection.Add(item);
                }
                return collection;
            }
            return null;
        }

        /// <summary>
        /// Gets the full path to folder into path to root for container in recent locations.
        /// </summary>
        /// <param name="path">Path to folder.</param>
        /// <param name="rootNodeName">Name of the root node.</param>
        /// <returns>String with path.</returns>
        public static List<string> ConvertToRecentItemPathToRoot(string[] path, string rootNodeName)
        {
            if (path?.Any() != true)
            {
                throw new ArgumentException($"Given path is null or empty.");
            }

            var fullPath = new List<string>();
            fullPath.Add(rootNodeName);

            fullPath.AddRange(path);
            fullPath.Remove(FoldersContainerName);

            var preparedPath = fullPath.Select(i =>
            {
                var word = SpecialPathItemsParts.Find(w => i.StartsWith(w));
                if (word != null)
                {
                    Regex regex = new Regex(word);
                    return regex.Replace(i, "", 1).TrimStart();
                }
                return i;
            }).ToList();

            return preparedPath;
        }
    }
}
