using System.Collections.Generic;
using System.Linq;

namespace NetDocuments.Automation.Helpers.Extensions
{
    /// <summary>
    /// Extension class for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Determines that two collections are equal by their count and elements equal.
        /// </summary>
        /// <remarks>More details on http://ideone.com/CSRMdc. </remarks>
        /// <typeparam name="T">The type of object to comparer.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="other">The target collection.</param>
        /// <param name="comparer">
        /// The System.Collections.Generic.IEqualityComparer`1 implementation to use when
        /// comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1
        /// for the type of the key.
        /// </param>
        /// <returns>True if two collections are equal by their count and elements; otherwise, false.</returns>
        public static bool AreEqual<T>(this IEnumerable<T> source, IEnumerable<T> other, IEqualityComparer<T> comparer = null)
        {
            if (source == null)
            {
                return other == null;
            }

            if (other == null)
            {
                return false;
            }

            var sourceLength = source.Count();
            var otherLength = other.Count();

            if (sourceLength != otherLength)
            {
                return false;
            }

            // Create dictionary.
            // Keys - 'source' elements.
            // Values - their counts in the 'source'.
            Dictionary<T, int> counts = comparer == null
                ? new Dictionary<T, int>(sourceLength)
                : new Dictionary<T, int>(sourceLength, comparer);

            // Fill the dictionary with 'source' items and their counts:
            foreach (T item in source)
            {
                if (counts.TryGetValue(item, out int value))
                {
                    // If item already exists, add 1 to counts.
                    counts[item] = value + 1;
                }
                else
                {
                    // Add new item with count = 1.
                    counts.Add(item, 1);
                }
            }

            // Remove 'other' items from the dictionary:
            foreach (T item in other)
            {
                if (counts.TryGetValue(item, out int value))
                {
                    if (value == 1)
                    {
                        // If the count == 1, remove the item from the dictionary
                        counts.Remove(item);
                    }
                    else
                    {
                        // If the item exists, decrease its count:
                        counts[item] = value - 1;
                    }
                }
                else
                {
                    // if no such item, collections are not equal.
                    return false;
                }
            }

            // If there is no items in the dictionary, collections are equal.
            return counts.Count == 0;
        }
    }
}
