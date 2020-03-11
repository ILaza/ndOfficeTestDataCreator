using NetDocuments.Client.COM.Models;

namespace NetDocuments.Automation.Helpers.Comparers
{
    /// <summary>
    /// ProfileAttribute objects comparer.
    /// </summary>
    public class ProfileAttributeComparer : ComparerBase<ProfileAttribute>
    {
        /// <summary>
        /// Compares the specified object is equal to the current object.
        /// </summary>
        /// <param name="x">The current object to compare with.</param>
        /// <param name="y">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        protected override bool Compare(ProfileAttribute x, ProfileAttribute y)
        {
            return x.Id == y.Id
                   && x.Value.Equals(y.Value);
        }
    }
}
