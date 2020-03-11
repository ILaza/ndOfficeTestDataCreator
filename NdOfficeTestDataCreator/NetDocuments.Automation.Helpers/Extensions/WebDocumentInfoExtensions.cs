using NetDocuments.Automation.Helpers.Entities;

namespace NetDocuments.Automation.Helpers.Extensions
{
    /// <summary>
    /// Extension class for WebDocumentInfo entity.
    /// </summary>
    public static class WebDocumentInfoExtensions
    {
        /// <summary>
        /// Converts document info into a string in a MS Office app title format.
        /// </summary>
        /// <param name="document">Document info.</param>
        /// <param name="versionNumber">Document version number.</param>
        /// <param name="formatString">Format string.</param>
        /// <returns>A string representation of MS Office app title.</returns>
        public static string ConvertToMsOfficeTitle(this WebDocumentInfo document, int versionNumber, string formatString)
        {
            var versionInfo = document.Versions.Find(v => v.Number.Equals(versionNumber));
            return string.Format(formatString, document.Name, versionInfo.VersionType, document.Id, versionInfo.Number);
        }

        /// <summary>
        /// Converts document info into a string in a MS Office app title format.
        /// </summary>
        /// <param name="document">Document info.</param>
        /// <param name="versionLabel">Document version label.</param>
        /// <param name="formatString">Format string.</param>
        /// <returns>A string representation of MS Office app title.</returns>
        public static string ConvertToMsOfficeTitle(this WebDocumentInfo document, string versionLabel, string formatString)
        {
            var versionInfo = document.Versions.Find(v => v.VersionLabel.Equals(versionLabel));
            return string.Format(formatString, document.Name, versionInfo.VersionType, document.Id, versionInfo.Number);
        }
    }
}
