using System;
using System.Collections.Generic;

using NetDocuments.Rest.Contracts.Models.V1;

namespace NetDocuments.Automation.Helpers.Entities
{
    public sealed class WebDocumentInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public (string nodeId, string nodeName)[] Path { get; set; }

        public WebVersionInfo OfficialVersion => Versions?.Find(v => v.IsOfficial);

        public List<WebVersionInfo> Versions { get; set; }

        public List<AttachmentInfo> Attachments { get; set; }

        // TODO: change NdDocumentAttribute to entity with PA name field.
        public List<NdDocumentAttribute> Attributes { get; set; }

        public bool IsOnHold { get; set; }

        public bool CheckedIn { get; set; }

        public string Envelope { get; set; }

        public WebDocumentInfo()
        {
            Versions = new List<WebVersionInfo>();
            Attachments = new List<AttachmentInfo>();
            Attributes = new List<NdDocumentAttribute>();
        }

        public WebVersionInfo GetVersionByNumber(int number)
            => Versions?.Find(v => v.Number == number);

        public WebVersionInfo GetVersionByLabel(string versionLabel)
           => Versions?.Find(v => v.VersionLabel == versionLabel);

        public void SetOfficialVersionTo(int versionNumber)
        {
            var version = GetVersionByNumber(versionNumber);

            if (version != null && version.IsOfficial == false)
            {
                OfficialVersion.IsOfficial = false;
                version.IsOfficial = true;
            }
        }

        public string ToNdOfficeFileName(int versionNumber)
        {
            var version = GetVersionByNumber(versionNumber)
                ?? throw new InvalidOperationException($"Version {versionNumber} does not exist in document {Id}");

            return $"{Name} {Id} v.{version.Number}.{version.VersionType}";
        }

        public string ToMsOfficeTitle(string formatString, int versionNumber)
        {
            var version = GetVersionByNumber(versionNumber)
                ?? throw new InvalidOperationException($"Version {versionNumber} does not exist in document {Id}");

            return string.Format(formatString, Name, version.VersionType, Id, version.VersionLabel);
        }

        public string ConvertNameToACFormat(int versionNumber)
        {
            var version = GetVersionByNumber(versionNumber)
                ?? throw new InvalidOperationException($"Version {versionNumber} does not exist in document {Id}");

            return $"{Name}.{version.VersionType} v.{version.VersionLabel}";
        }

        public string ConvertNameToACFormat(string versionLabel)
        {
            var version = GetVersionByLabel(versionLabel)
                ?? throw new NullReferenceException($"Version {versionLabel} does not exist in document {Id}");

            return $"{Name}.{version.VersionType} v.{versionLabel}";
        }

        /// <summary>
        /// Copies new instance of current object into new one.
        /// </summary>
        /// <returns>Copy of the current object.</returns>
        public object Clone()
        {
            var newDocInfo = new WebDocumentInfo
            {
                Id = this.Id,
                Name = this.Name,
                Path = ((string nodeId, string nodeName)[])this.Path.Clone(),
                IsOnHold = this.IsOnHold,
                CheckedIn = this.CheckedIn,
                Envelope = this.Envelope,
                Versions = new List<WebVersionInfo>(),
                Attachments = new List<AttachmentInfo>()
            };

            foreach (var version in Versions)
            {
                newDocInfo.Versions.Add((WebVersionInfo)version.Clone());
            }

            foreach (var attachment in Attachments)
            {
                newDocInfo.Attachments.Add((AttachmentInfo)attachment.Clone());
            }

            return newDocInfo;
        }
    }
}
