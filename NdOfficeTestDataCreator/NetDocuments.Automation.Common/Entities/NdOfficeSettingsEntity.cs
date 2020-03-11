using System;
using System.Collections.Generic;
using System.IO;

using NetDocuments.Automation.Common.Settings;

namespace NetDocuments.Automation.Common.Entities
{
    /// <summary>
    /// Class for holding NdOffice settings data.
    /// </summary>
    public class NdOfficeSettingsEntity
    {
        /// <summary>
        /// Full local echo path for the logged in user.
        /// </summary>
        public string EchoFileLocation
            => Path.Combine(EchoFolderPath, LoginTestData.UserId);

        /// <summary>
        /// Full local path to the ndOfice.exe file.
        /// </summary>
        public string NdOfficePath { get; private set; }

        /// <summary>
        /// Echo folder path.
        /// </summary>
        public string EchoFolderPath { get; private set; }

        /// <summary>
        /// Backup folder path.
        /// </summary>
        public string BackupFolderPath
            => Path.Combine(EchoFileLocation, NdOfficeConstants.BACK_UP_FOLDER_NAME);

        /// <summary>
        /// Attachments folder path.
        /// </summary>
        public string AttachmentsFolderPath
            => Path.Combine(EchoFileLocation, NdOfficeConstants.ATTACHMENTS_FOLDER_NAME);

        /// <summary>
        /// Comparisons folder path.
        /// </summary>
        public string ComparisonsFolderPath
            => Path.Combine(EchoFileLocation, NdOfficeConstants.COMPARISONS_FOLDER_NAME);

        /// <summary>
        /// Gets paths for all subfolders inside echo folder.
        /// </summary>
        /// <return>Collection of paths for subfolders.</return>
        public IEnumerable<string> GetEchoFolderSubfoldersPaths
            => Array.AsReadOnly(new[] { BackupFolderPath, AttachmentsFolderPath, ComparisonsFolderPath });

        /// <summary>
        /// Holds testing data for Login method.
        /// </summary>
        public LoginTestData LoginTestData { get; set; }

        /// <summary>
        /// Initializes new instance of the NdOfficeSettingsEntity class.
        /// </summary>
        /// <param name="applicationPath">Path to NDOffice application.</param>
        public NdOfficeSettingsEntity(string applicationPath)
        {
            NdOfficePath = applicationPath;
            EchoFolderPath = Path.Combine(@"C:\Users\", Environment.UserName, NdOfficeConstants.ECHO_FOLDER_NAME);
        }
    }
}