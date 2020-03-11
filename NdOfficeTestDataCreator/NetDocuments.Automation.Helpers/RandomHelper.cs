using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Helper class for getting random values.
    /// </summary>
    public static class RandomHelper
    {
        private static readonly Random random = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Returns a random string value.
        /// </summary>
        public static string GetRandomString()
        {
            return Path.GetRandomFileName();
        }

        public static IEnumerable<char> GetRandomContentPerformance(int fileSize)
        {
            const string chars = "AB CDEF GHIJKLM NOPQRSTUVWXY Z0123456789 abcdefghijk lmnopq rstuvwxyz";
            char[] data = new char[fileSize];

            data = (Enumerable.Repeat(chars, fileSize)
                              .Select(s => s[random.Next(s.Length)])
                              .ToArray());
            return data;
        }

        /// <summary>
        /// Returns a random int value.
        /// </summary>
        /// <param name="maxValue">Specified maximum.</param>
        /// <returns>Returns a non-negative random integer that is less than the specified maximum.</returns>
        public static int GetRandomInt(int maxValue)
        {
            return random.Next(maxValue);
        }

        /// <summary>
        /// Returns a random string value.
        /// </summary>
        /// <param name="length">Number of characters.</param>
        /// <returns>Returns random string with required length.</returns>
        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                                        .Select(s => s[random.Next(s.Length)])
                                        .ToArray());
        }

        /// <summary>
        /// Returns a random int value.
        /// </summary>
        /// <param name="minValue">Specified minimum.</param>
        /// <param name="maxValue">Specified maximum.</param>
        /// <returns>Returns a random integer that is within a specified range.</returns>
        public static int GetRandomInt(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a collection with given number of random strings.
        /// </summary>
        /// <param name="count">Strings count.</param>
        /// <returns>Collection of random strings.</returns>
        public static string[] GetRandomStrings(int count)
        {
            if (count < 1)
            {
                count = 1;
            }

            return Enumerable.Repeat(1, count)
                             .Select(_ => GetRandomString())
                             .ToArray();
        }

        public static T GetRandomItemFromCollection<T>(IEnumerable<T> collection)
        {
            if (collection.Any())
            {
                return collection.Skip(random.Next(0, collection.Count() - 1)).Take(1).Single();
            }

            return default(T);
        }

        /// <summary>
        /// Returns a List with unique random items.
        /// </summary>
        /// <param name="collection">Original collection.</param>
        /// <param name="itemsCount">Count of unique random items for return.</param>
        /// <returns>List with unique random elements.</returns>
        public static ICollection<T> GetRandomUniqueItemsFromCollection<T>(ICollection<T> collection, int itemsCount)
        {
            var uniqueElements = GetRandomUniqueInts(itemsCount, collection.Count);
            var items = new List<T>();

            foreach(var element in uniqueElements)
            {
                items.Add(collection.ElementAt(element));
            }

            return items;
        }

        /// <summary>
        /// Returns a List with unique random numbers from given range.
        /// </summary>
        /// <param name="count">Count of returned items.</param>
        /// <param name="maxValue">Specified maximum for random range.</param>
        /// <returns>List with unique random numbers from given range. Throw an exception when maxValue is less then count.</returns>
        public static List<int> GetRandomUniqueInts(int count, int maxValue)
        {
            if (maxValue < count)
            {
                throw new ArgumentException("Max value for random range is less then count of returned items.");
            }

            var uniqueInts = new HashSet<int>();

            do
            {
                uniqueInts.Add(GetRandomInt(maxValue));
            }
            while (uniqueInts.Count < count);

            return uniqueInts.ToList();
        }
    }
}
