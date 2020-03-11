using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using NetDocuments.Automation.Helpers.Entities;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Helper for EMCloud logs.
    /// </summary>
    public static class EMCloudLogsHelper
    {
        /// <summary>
        /// Message about successfully filig.
        /// </summary>
        public static string SuccessfullyFilingMessage = "Successfully filed email";

        /// <summary>
        /// Path to EmCloudApplicationJournal logs.
        /// </summary>
        /// <returns>Path to EmCloudApplicationJournal.</returns>
        public static string PathToJournalFile => Path.Combine(GetPathToLogsFolder(), "EmCloudApplicationJournal.log");

        /// <summary>
        /// Returns information about last successful filing.
        /// </summary>
        /// <returns>Information about last successful filed item.</returns>
        public static EMCloudFiledItem GetInformationAboutLastSucessfulFiling()
        {
            var emailList = ReadLogFile(PathToJournalFile).ToList();

            var lines = emailList.Select(item => JsonConvert.DeserializeObject<EMCloudFiledItem>(item))
                                 .Where(item => item.Action.Equals(SuccessfullyFilingMessage))
                                 .OrderBy(a => a.Date);

            return lines.LastOrDefault();
        }

        private static IEnumerable<string> ReadLogFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(stream))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static string GetPathToLogsFolder()
        {
            string name = Environment.UserName;
            return $@"C:\Users\{name}\AppData\Local\EmCloud\Logs\";
        }
    }
}