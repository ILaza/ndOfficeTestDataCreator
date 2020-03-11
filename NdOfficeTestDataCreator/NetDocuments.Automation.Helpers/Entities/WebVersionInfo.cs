using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDocuments.Automation.Helpers.Entities
{
    public sealed class WebVersionInfo
    {
        public int Number { get; set; }

        public string VersionLabel { get; set; }

        public string VersionType { get; set; }

        public string[] NormalizedContent { get; set; }

        public IEnumerable<char> NormalizedContentPerformance { get; set; }

        public bool IsOfficial { get; set; }

        public bool IsLocked { get; set; }

        public bool IsApproved { get; set; }

        public string Description { get; set; }

        public void AddContent(IEnumerable<string> content)
        {
            if (content == null)
            {
                return;
            }

            if (NormalizedContent == null || !NormalizedContent.Any())
            {
                NormalizedContent = content.ToArray();
            }
            else
            {
                var newContentCollection = new List<string>();
                newContentCollection.AddRange(NormalizedContent);
                newContentCollection.AddRange(content);
                NormalizedContent = newContentCollection.ToArray();
            }
        }
        public void AddContentPerformance(IEnumerable<char> content)
        {
            if (content == null)
            {
                return;
            }

            if (NormalizedContentPerformance == null || !NormalizedContentPerformance.Any())
            {
                NormalizedContentPerformance = content;
            }
            else
            {
                char[] newContentCollection = NormalizedContentPerformance.ToArray();
                content.ToArray().CopyTo(newContentCollection, newContentCollection.Length);
                NormalizedContentPerformance = newContentCollection;

                //NormalizedContentPerformance.ToList().AddRange(content);
            }
        }

        public void ReplaceContent(IEnumerable<string> content)
        {
            if (content == null || !content.Any())
            {
                NormalizedContent = new string[0];
            }
            else
            {
                var copyContent = content.ToArray();
                NormalizedContent = new string[copyContent.Length];
                Array.Copy(copyContent, NormalizedContent, copyContent.Length);
            }
        }

        public object Clone()
        {
            var newVersionInfo = new WebVersionInfo
            {
                Number = this.Number,
                VersionLabel = this.VersionLabel,
                VersionType = this.VersionType,
                NormalizedContent = (string[]) this.NormalizedContent.Clone(),
                IsOfficial = this.IsOfficial,
                IsLocked = this.IsLocked,
                IsApproved = this.IsApproved,
                Description = this.Description,
            };

            return newVersionInfo;
        }
    }
}
