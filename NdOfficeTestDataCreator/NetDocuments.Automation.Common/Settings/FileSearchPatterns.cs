using System.Collections.Generic;

namespace NetDocuments.Automation.Common.Settings
{
    /// <summary>
    /// Holds file search string patterns.
    /// </summary>
    public class FileSearchPatterns
    {
        /// <summary>
        /// Holds collection of search patterns.
        /// </summary>
        public static Dictionary<string, string> SearchPatternsCollection => new Dictionary<string, string>
        {
            { "docx", DOCX },
            { "docm", DOCM },
            { "doc", DOC },
            { "dotx", DOTX },
            { "dotm", DOTM },
            { "dot", DOT },
            { "rtf", RTF },

            { "xlsx", XLSX },
            { "pptx", PPTX },
            { "pdf",  PDF  },
        };

        /// <summary>
        /// Search pattern for "docx" files.
        /// </summary>
        public const string DOCX = "*.docx";

        /// <summary>
        /// Search pattern for "docm" files.
        /// </summary>
        public const string DOCM = "*.docm";

        /// <summary>
        /// Search pattern for "doc" files.
        /// </summary>
        public const string DOC = "*.doc";

        /// <summary>
        /// Search pattern for "dotx" files.
        /// </summary>
        public const string DOTX = "*.dotx";

        /// <summary>
        /// Search pattern for "dotm" files.
        /// </summary>
        public const string DOTM = "*.dotm";

        /// <summary>
        /// Search pattern for "dot" files.
        /// </summary>
        public const string DOT = "*.dot";

        /// <summary>
        /// Search pattern for "rtf" files.
        /// </summary>
        public const string RTF = "*.rtf";

        /// <summary>
        /// Search pattern for "xlsx" files.
        /// </summary>
        public const string XLSX = "*.xlsx";

        /// <summary>
        /// Search pattern for "pdf" files.
        /// </summary>
        public const string PDF = "*.pdf";

        /// <summary>
        /// Search pattern for "pptx" files.
        /// </summary>
        public const string PPTX = "*.pptx";
    }
}
