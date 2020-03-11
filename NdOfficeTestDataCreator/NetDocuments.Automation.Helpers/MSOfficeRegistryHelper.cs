using System;
using System.Linq;
using System.Security;

using Microsoft.Win32;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Holds method to work with MS Office registry settings.
    /// </summary>
    public static class MSOfficeRegistryHelper
    {
        /// <summary>
        /// MS Office general settings registry path format.
        /// </summary>
        private const string MS_OFFICE_GENERAL_SETTINGS_REGISTRY_PATH_FORMAT = @"Software\Microsoft\Office\{0}\Common\General";

        /// <summary>
        /// Word application version registry path.
        /// </summary>
        private const string WORD_APP_VERSION_REGISTRY_PATH = @"Word.Application\CurVer";

        /// <summary>
        /// MS Office backstage registry property.
        /// </summary>
        private const string SKIP_OPEN_AND_SAVE_AS_PLACE_REGISTRY_PROPERTY = "SkipOpenAndSaveAsPlace";

        /// <summary>
        /// Enable or disable MS Office backstage.
        /// </summary>
        /// <param name="isEnabled">Enable backstage if true, disable backstage if false.</param>
        public static void SetMSOfficeBackstageState(bool isEnabled)
        {
            var registryValue = isEnabled ? 0 : 1;

            try
            {
                SetOrCreateIfNotExists(RegistryHive.CurrentUser, SKIP_OPEN_AND_SAVE_AS_PLACE_REGISTRY_PROPERTY, registryValue);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Cannot change backstage state to {isEnabled} because of {ex.Message}.");
                Console.WriteLine(ex);
                throw;
            }

            var script = "gpupdate /force";
            PowerShellHelper.ExecutePowerShellScript(script);
        }

        /// <summary>
        /// Get MS Office version from Word application.
        /// </summary>
        public static string GetMsOfficeVersionFromRegistry()
        {
            return RegistryHelper.GetRegistryValue<string>(RegistryHive.ClassesRoot,
                                                           WORD_APP_VERSION_REGISTRY_PATH,
                                                           propertyName: null,
                                                           wow6432: false);
        }

        /// <summary>
        /// Sets the value of a name/value pair in the registry key, using the specified
        /// registry data type, or creates a new name/value pair in the registry key if it doesn't exist.
        /// </summary>
        /// <param name="registryHive">Registry hive to get value.</param>
        /// <param name="propertyName">Property name. This string is not case-sensitive.</param>
        /// <param name="value">Property value.</param>
        public static void SetOrCreateIfNotExists<T>(RegistryHive registryHive, string propertyName, T value)
        {
            var registryVersion = GetMsOfficeVersionFromRegistry();

            if (string.IsNullOrEmpty(registryVersion))
            {
                throw new InvalidOperationException("Cannot get MS Word version from registry.");
            }

            var version = registryVersion.Split('.').Last();

            if (!int.TryParse(version, out var wordVersion))
            {
                throw new InvalidOperationException($"Cannot get MS Word version from string {version}.");
            }

            var path = $"{wordVersion}.0";
            var registryPath = string.Format(MS_OFFICE_GENERAL_SETTINGS_REGISTRY_PATH_FORMAT, path);

            try
            {
                if (RegistryHelper.IsRegistrySubkeyExists(registryHive, registryPath, wow6432: false))
                {
                    RegistryHelper.SetRegistryValue(registryHive,
                                                    registryPath,
                                                    propertyName,
                                                    value,
                                                    wow6432: false,
                                                    createValueIfNotExists: true);
                }
                else
                {
                    RegistryHelper.CreateSubKey(registryHive,
                                                registryPath,
                                                propertyName,
                                                value);
                }
            }
            catch (SecurityException ex)
            {
                throw new SecurityException($"Current Windows user: <{Environment.UserName}> doesn't have rights to set " +
                                            $"the given property: <{propertyName}> in the <{registryHive}> hive.", ex);
            }
        }
    }
}
