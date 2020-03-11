using System;
using System.Collections.Generic;
using System.Linq;

using NetDocuments.Client.COM.Models;
using NetDocuments.Rest.Contracts.Models.V1;

namespace NetDocuments.Automation.Helpers.Extensions
{
    /// <summary>
    /// Extensions class for NetDocuments.Rest.Client.DTO.NdDocumentAttribute objects.
    /// </summary>
    public static class NdDocumentAttributeExtension
    {
        /// <summary>
        /// Converts NetDocuments.Rest.Client.DTO.NdDocumentAttribute object to NetDocuments.Client.COM.Models.ProfileAttribute.
        /// </summary>
        /// <param name="attribute">NetDocuments.Rest.Client.DTO.NdDocumentAttribute object to convert.</param>
        /// <returns>NetDocuments.Client.COM.Models.ProfileAttribute object.</returns>
        public static ProfileAttribute ToProfileAttribute(this NdDocumentAttribute attribute)
        {
            var profileAttribute = new ProfileAttribute
            {
                Id = attribute.Id
            };

            if (attribute.Values?.FirstOrDefault() != null)
            {
                profileAttribute.Value = attribute.Values.FirstOrDefault();
            }
            else if (attribute.Value != null)
            {
                profileAttribute.Value = attribute.Value;
            }
            else
            {
                profileAttribute.Value = attribute.DateValue.ToShortDateString();
            }

            return profileAttribute;
        }

        /// <summary>
        /// Converts the list of NetDocuments.Rest.Client.DTO.NdDocumentAttribute objects
        /// to list of  NetDocuments.Client.COM.Models.ProfileAttribute objects.
        /// </summary>
        /// <param name="attributes">List of NetDocuments.Rest.Client.DTO.NdDocumentAttribute objects to convert.</param>
        /// <returns>Collection of NetDocuments.Client.COM.Models.ProfileAttribute objects</returns>
        public static IEnumerable<ProfileAttribute> ToProfileAttributesCollection(this IEnumerable<NdDocumentAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                yield return attribute.ToProfileAttribute();
            }
        }

        /// <summary>
        /// Compares two PAs by their Ids and PAs values.
        /// </summary>
        /// <param name="self">A PA to compare.</param>
        /// <param name="attribute">Another PA to compare.</param>
        /// <returns></returns>
        public static bool IsEqualTo(this NdDocumentAttribute self, NdDocumentAttribute attribute)
        {
            if (self == null || attribute == null)
            {
                throw new ArgumentNullException("Aguments shouldn't be null.");
            }

            return self.Id.Equals(attribute.Id)
                   && self.Values.Count.Equals(attribute.Values.Count)
                   && self.Values.SequenceEqual(attribute.Values)
                   && self.DateValue.Equals(attribute.DateValue);
        }

        /// <summary>
        /// Compares two PAs collections by their Ids and PAs values.
        /// </summary>
        /// <param name="self">A PA collection to compare.</param>
        /// <param name="attribute">Another PA collection to compare.</param>
        /// <returns></returns>
        public static bool IsSequanceEqualTo(this IEnumerable<NdDocumentAttribute> actual, IEnumerable<NdDocumentAttribute> expected)
            => actual.Zip(expected, (a, e) => a.IsEqualTo(e)).All(a => a.Equals(true));
    }
}
