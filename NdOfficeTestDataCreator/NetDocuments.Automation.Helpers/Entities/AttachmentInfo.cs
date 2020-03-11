using System;

namespace NetDocuments.Automation.Helpers.Entities
{
    /// <summary>
    /// Class for holding attachment specific info.
    /// </summary>
    public class AttachmentInfo : ICloneable
    {
        /// <summary>
        /// Returns attachment name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns attachment extension.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Formats attachment info into MS Office title.
        /// </summary>
        /// <param name="formatString">String with ndOffice file name format.</param>
        /// <returns>Formatted file name.</returns>
        public string ToMsOfficeTitle(string format)
            => string.Format(format, Name, Extension);

        /// <summary>
        /// Copies new instance of current object into new one.
        /// </summary>
        /// <returns>Copy of the current object.</returns>
        public object Clone()
        {
            return new AttachmentInfo
            {
                Name = this.Name,
                Extension = this.Extension
            };
        }
    }
}
