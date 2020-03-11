using System;
using System.Diagnostics;
using System.IO;

using Microsoft.Win32;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Class for holding methods for obtaining MS Office version.
    /// </summary>
    public static class OfficeVersionHelper
    {
        private const string APP_PATH_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";

        /// <summary>
        /// Obtains MS Office version.
        /// </summary>
        /// <returns>MS Office version.</returns>
        public static string ObtainMsOfficeVersion(string appName)
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(ObtainApplicationPath(appName),
                                                                 $"{appName}.exe"));
                return versionInfo.ProductMajorPart.ToString();
            }
            catch (FileNotFoundException)
            {
                throw new ArgumentException("Wrong application name");
            }
        }

        /// <summary>
        /// Obtains MS Office application path.
        /// </summary>
        /// <returns>MS Office application path.</returns>
        public static string ObtainApplicationPath(string appName)
        {
            var appKeyPath = Path.Combine(APP_PATH_KEY, $"{appName}.exe");
            return RegistryHelper.GetRegistryValue<string>(RegistryHive.LocalMachine, appKeyPath, "Path", false);
        }

        /// <summary>
        /// Obtains full MS Office app name, version and bitness.
        /// </summary>
        /// <param name="processName">Process name of application.</param>
        /// <returns>Full name and version with bitness.</returns>
        public static string ObtainMsOfficeProductNameAndVersion(string processName)
        {
            try
            {
                var appPath = Path.Combine(ObtainApplicationPath(processName), $"{processName}.exe");
                var versionInfo = FileVersionInfo.GetVersionInfo(appPath);
                return $"{versionInfo.FileDescription} ({versionInfo.ProductVersion}) {WindowsSystemHelper.GetBitness(appPath)}";
            }
            catch (FileNotFoundException)
            {
                throw new ArgumentException("Wrong process name.");
            }
        }

        /// <summary>
        /// Obtains MS Office product version and bitness.
        /// </summary>
        /// <param name="processName">Process name of application.</param>
        /// <returns>Product version with bitness.</returns>
        public static string ObtainMsOfficeProductVersion(string processName)
        {
            try
            {
                var appPath = Path.Combine(ObtainApplicationPath(processName), $"{processName}.exe");
                var versionInfo = FileVersionInfo.GetVersionInfo(appPath);
                return $"{versionInfo.ProductVersion} {WindowsSystemHelper.GetBitness(appPath)}";
            }
            catch (FileNotFoundException)
            {
                throw new ArgumentException("Wrong process name.");
            }
        }
    }
}