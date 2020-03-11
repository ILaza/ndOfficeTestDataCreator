using System.Collections.Generic;

namespace NetDocuments.Automation.Helpers.Comparers
{
    /// <summary>
    /// Base objects comparer.
    /// </summary>
    public abstract class ComparerBase<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="x">The current object to compare with.</param>
        /// <param name="y">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            if (EqualityComparer<T>.Default.Equals(x, default(T)))
            {
                return EqualityComparer<T>.Default.Equals(y, default(T));
            }

            if (EqualityComparer<T>.Default.Equals(y, default(T)))
            {
                return false;
            }

            return Compare(x, y);
        }

        /// <summary>
        /// Gets hash value.
        /// </summary>
        /// <param name="obj">The T object.</param>
        /// <returns>A hash code for the current object.</returns>
        public virtual int GetHashCode(T obj)
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Compares the specified object is equal to the current object.
        /// </summary>
        /// <param name="x">The current object to compare with.</param>
        /// <param name="y">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        protected abstract bool Compare(T x, T y);
    }
}
