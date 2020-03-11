using System;
using System.Diagnostics;
using System.IO;

using Microsoft.Win32;

namespace NetDocuments.Automation.Helpers.Utils
{
    // TODO: refactor this (ndOffice team)
    /// <summary>
    /// Represents ndOffice info reader class.
    /// </summary>
    public static class NdOfficeInfoReader
    {
        private const string ND_OFFICE_REGISTRY_PATH = @"Software\NetVoyage\NetDocuments";
        private const string ECHOING_CLIENT_PATH_VALUE_KEY = "EchoingClientPath";
        private const string ECHOING_FOLDER_PATH_VALUE_KEY = "EchoingFolderPath";

        /// <summary>
        /// Retrieves full path to the ndOffice.exe file with application from registry.
        /// Returns null if the ndOffice.exe file path does not exist in the registry.
        /// </summary>
        public static string NdOfficeFullPath { get { return GetNdOfficeKeyValue(ECHOING_CLIENT_PATH_VALUE_KEY); } }

        /// <summary>
        /// Retrieves full path to the ndOffice.exe file without application from registry.
        /// Returns null if the ndOffice.exe file path does not exist in the registry.
        /// </summary>
        public static string NdOfficePath { get { return Path.GetDirectoryName(GetNdOfficeKeyValue(ECHOING_CLIENT_PATH_VALUE_KEY)); } }

        /// <summary>
        /// Retrieves full path to the echoing folder from registry.
        /// Returns null if it does not exist in the registry.
        /// </summary>
        public static string EchoingFolderPath { get { return GetNdOfficeKeyValue(ECHOING_FOLDER_PATH_VALUE_KEY); } }

        /// <summary>
        /// Gets the value associated with the specified key name from current user ndOffice registry path.
        /// If the key name is not found, gets the value from local machine ndOffice registry path.
        /// If the key name is still not found, returns null.
        /// </summary>
        /// <param name="keyName">NdOffice's key name. This string is not case-sensitive.</param>
        /// <returns>The value associated with the specified ndOffice's key name.</returns>
        private static string GetNdOfficeKeyValue(string keyName)
        {
            var wow6432 = Environment.Is64BitOperatingSystem;
            var path = RegistryHelper.GetRegistryValue<string>(RegistryHive.CurrentUser,
                                                               ND_OFFICE_REGISTRY_PATH,
                                                               keyName,
                                                               wow6432);
            if (string.IsNullOrEmpty(path))
            {
                path = RegistryHelper.GetRegistryValue<string>(RegistryHive.LocalMachine,
                                                               ND_OFFICE_REGISTRY_PATH,
                                                               keyName,
                                                               wow6432);
            }
            return path;
        }

        /// <summary>
        /// Obtains full version of ndOffice.
        /// </summary>
        /// <returns>Full version of ndOffice.</returns>
        public static string ObtainNdOfficeVersion()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(NdOfficeFullPath);
            return versionInfo.FileVersion.ToString();
        }
    }
}