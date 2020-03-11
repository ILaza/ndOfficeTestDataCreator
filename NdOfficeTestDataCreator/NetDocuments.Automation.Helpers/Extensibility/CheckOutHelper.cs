using NetDocuments.Automation.Common.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocuments.Automation.Helpers.Extensibility
{
    public static class CheckOutHelper
    {
        public static string WaitForDocument(string path,
                                    string documentName,
                                    string searchPattern = FileSearchPatterns.DOCX,
                                    SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Wait.ForResult(() => Directory.GetFiles(path, searchPattern, searchOption)
                                                 ?.FirstOrDefault(i => i.Contains(documentName)),
                                  TimeConstants.MAX_WAIT_TIME,
                                  TimeConstants.SLEEP_TIME) ?? string.Empty;
        }

    }
}
