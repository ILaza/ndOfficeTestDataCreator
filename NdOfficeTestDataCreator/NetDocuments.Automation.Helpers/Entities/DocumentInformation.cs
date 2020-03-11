using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDocuments.Automation.Helpers.Entities
{
    /// <summary>
    /// Class for holding document specific info.
    /// </summary>
    public class DocumentInformation : ICloneable
    {
        /// <summary>
        /// Returns document id.
        /// </summary>
        public string Id { get; set; }

        public (string nodeId, string nodeName)[] Path { get; set; }

        /// <summary>
        /// Returns local path to file.
        /// </summary>
        public string LocalPathToFile { get; set; }

        /// <summary>
        /// Returns document version.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Returns document version name.
        /// </summary>
        public string VersionName { get; set; }

        /// <summary>
        /// Returns document versions number.
        /// </summary>
        public int VersionsNumber { get; set; }

        /// <summary>
        /// Returns document version label.
        /// </summary>
        public string VersionLabel { get; set; }

        /// <summary>
        /// Returns document version description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Returns document name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns document extension.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Returns document status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Returns document modified date time.
        /// </summary>
        public DateTime ModifiedDateTime { get; set; }

        /// <summary>
        /// Returns document attachment.
        /// </summary>
        public AttachmentInfo Attachment { get; set; }

        /// <summary>
        /// Returns document content.
        /// </summary>
        public string[] Content { get; set; }

        /// <summary>
        /// Formats document info in ndOffice file name format.
        /// </summary>
        /// <returns>Formatted file name.</returns>
        public string ToNdOfficeFileName() => $"{Name} {Id} v.{VersionLabel}.{Extension}";

        /// <summary>
        /// Formats document info in MS Office title.
        /// </summary>
        /// <param name="formatString">String with MS Office title format.</param>
        /// <returns>Formatted file name.</returns>
        public string ToMsOfficeTitle(string formatString)
            => string.Format(formatString, Name, Extension, Id, VersionLabel);

        public string ConvertNameToACFormat()
            => $"{Name}.{Extension} v.{VersionLabel}";

        /// <summary>
        /// Adds given content into existing.
        /// </summary>
        /// <param name="additionalData">Additional content.</param>
        public void AddContent(IEnumerable<string> additionalData)
        {
            if (additionalData != null)
            {
                if (Content == null)
                {
                    Content = additionalData.ToArray();
                }
                else
                {
                    var newContentCollection = new List<string>();
                    newContentCollection.AddRange(Content);
                    newContentCollection.AddRange(additionalData);
                    Content = newContentCollection.ToArray();
                }
            }
        }

        /// <summary>
        /// Copies new instance of current object into new one.
        /// </summary>
        /// <returns>Copy of the current object.</returns>
        public object Clone()
        {
            var newDocInfo = new DocumentInformation
            {
                Id = this.Id,
                Version = this.Version,
                VersionsNumber = this.VersionsNumber,
                VersionName = this.VersionName,
                VersionLabel = this.VersionLabel,
                Description = this.Description,
                Name = this.Name,
                Extension = this.Extension,
                Status = this.Status,
                ModifiedDateTime = this.ModifiedDateTime,
                Attachment = (AttachmentInfo)this.Attachment?.Clone(),
            };

            newDocInfo.AddContent(Content);
            return newDocInfo;
        }

        /// <summary>
        /// Formats document info in Outlook attachment name format.
        /// </summary>
        /// <returns>Outlook attachment name.</returns>
        public string ToOutlookAttachmentName() => $"{Name}_version_{VersionLabel}.{Extension}";
    }
}
