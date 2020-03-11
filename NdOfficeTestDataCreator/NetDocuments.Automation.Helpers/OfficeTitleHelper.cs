using System.Collections.Generic;
using System.Text.RegularExpressions;

using NetDocuments.Automation.Common.Exceptions.Automation;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Class for holding methods for parse title.
    /// </summary>
    public class OfficeTitleHelper
    {
        private static Dictionary<string, string> patternsForApplications = new Dictionary<string, string>
        {
            ["winword"] = @"((?<docName>(.+))\.(?<docExt>(\w+)) (?<docId>(\d{4}-\d{4}-\d{4})) v.(?<docVer>(\d+)))( - (?<appName>(.*)))?",
            ["excel"] = @"((?<docName>(.+))\.(?<docExt>(\w+)) (?<docId>(\d{4}-\d{4}-\d{4})) v.(?<docVer>(\d+)))( - (?<appName>(.*)))?",
            ["powerpnt"] = @"((?<docName>(.+)) (?<docId>(\d{4}-\d{4}-\d{4})) v.(?<docVer>(\d+))(\.(?<docExt>(\w+)))?)( - (?<appName>(.*)))?",
        };

        // Note: Title formats:
        // document_for_open_a_version_from_version_dialog.xlsx 4839-2902-6378 v.2 - Excel
        // document_for_open_a_version_from_version_dialog 4832-2438-3306 v.2.pptx - PowerPoint
        // document_for_open_a_version_from_version_dialog 4832-2438-3306 v.2 - Microsoft PowerPoint

        // Note: Word/Excel format
        private const string regexPattern = @"(((?<docName>(.+))\.(?<docExt>(\w+)) (?<docId>(\d{4}-\d{4}-\d{4})) v.(?<docVer>(\d+)))|" +
                                            // PowerPoint format
                                            @"((?<docName>(.+)) (?<docId>(\d{4}-\d{4}-\d{4})) v.(?<docVer>(\d+))(\.(?<docExt>(\w+)))?))" +
                                            // App name [optional]
                                            @"( - (?<appName>(.*)))?";

        private Match regexMatch;

        /// <summary>
        /// Parses the given title.
        /// </summary>
        /// <param name="title">Title string.</param>
        public void ParseTitle(string title)
        {
            if (title == null)
            {
                throw new ParseException("Cannot parse document title. Title is null.");
            }

            regexMatch = Regex.Match(title, regexPattern);
        }

        /// <summary>
        /// Parses the given title with specified regex pattern.
        /// </summary>
        /// <param name="title">Title string.</param>
        /// <param name="regexPattern">Regex pattern.</param>
        public void ParseTitle(string title, string regexPattern)
        {
            regexMatch = Regex.Match(title, regexPattern);
        }

        /// <summary>
        /// Returns document name.
        /// </summary>
        public string DocumentName => regexMatch?.Groups["docName"].Value;

        /// <summary>
        /// Returns document extension.
        /// </summary>
        public string DocumentExtension => regexMatch?.Groups["docExt"].Value;

        /// <summary>
        /// Returns document Id.
        /// </summary>
        public string DocumentId => regexMatch?.Groups["docId"].Value;

        /// <summary>
        /// Returns document version.
        /// </summary>
        public string DocumentVersion => regexMatch?.Groups["docVer"].Value;

        /// <summary>
        /// Returns document version label.
        /// </summary>
        public string DocumentVersionLabel => regexMatch?.Groups["docVer"].Value;

        /// <summary>
        /// Parse document version.
        /// </summary>
        /// <returns>Document version if parse was successful; otherwise 0.</returns>
        public int GetDocumentVersion()
        {
            if (!int.TryParse(DocumentVersion, out int version))
            {
                throw new ParseException("Cannot get document version from document title.");
            }
            return version;
        }

        /// <summary>
        /// Returns application name.
        /// </summary>
        public string AppName => regexMatch?.Groups["appName"].Value;
    }
}
