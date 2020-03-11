using System;
using System.Collections.Generic;
using System.Security;

using Microsoft.Win32;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Holds method to work with NdOffice registry settings.
    /// </summary>
    public static class NdOfficeRegistryHelper
    {
        private static bool Wow6432 = Environment.Is64BitOperatingSystem;

        /// <summary>
        /// NetDocuments registry tree path.
        /// </summary>
        public const string NETVOYAGE_REGISTRY_TREE_PATH = @"Software\NetVoyage";

        /// <summary>
        /// ndOffice registry path.
        /// </summary>
        public const string ND_OFFICE_REGISTRY_PATH = @"Software\NetVoyage\NetDocuments";

        /// <summary>
        /// Set that contains ndOffice settings properties names.
        /// </summary>
        private static ISet<string> RegistryPropertiesNames
            => new HashSet<string>(typeof(NdOfficeRegistryPropertiesNames).GetEnumNames());

        /// <summary>
        /// Gets host name value in the HKCU.
        /// </summary>
        public static string GetHostNameHKCU()
        {
            return GetRegistryValue<string>(RegistryHive.CurrentUser, NdOfficeRegistryPropertiesNames.Host.ToString());
        }

        /// <summary>
        /// Sets host name value in the HKCU.
        /// </summary>
        /// <param name="hostName">Host name to login.</param>
        public static void SetHostNameHKCU(string hostName)
        {
            SetOrCreateIfNotExists(RegistryHive.CurrentUser, NdOfficeRegistryPropertiesNames.Host.ToString(), hostName);
        }

        /// <summary>
        /// Sets workspace default view value in the HKCU.
        /// </summary>
        /// <param name="workspaceDefaultView">Workspace default view value.</param>
        public static void SetWorkspaceDefaultViewHKCU(string workspaceDefaultView)
        {
            SetOrCreateIfNotExists(RegistryHive.CurrentUser,
                                   NdOfficeRegistryPropertiesNames.WorkspacesDefaultView.ToString(),
                                   workspaceDefaultView);
        }

        /// <summary>
        /// Sets TracingEnabled value in the HKCU.
        /// </summary>
        /// <param name="value">Enabled or not enabled?</param>
        public static void SetTracingEnabledHKCU(string value)
        {
            SetOrCreateIfNotExists(RegistryHive.CurrentUser, NdOfficeRegistryPropertiesNames.TracingEnabled.ToString(), value);
        }

        /// <summary>
        /// Determines if ndOffice registry sub key exists for the current user.
        /// </summary>
        /// <returns>True if ndOffice registry sub key exists; otherwise false.</returns>
        public static bool IsNdOfficeRegistrySubkeyExists(RegistryHive registryHive)
        {
            return RegistryHelper.IsRegistrySubkeyExists(registryHive,
                                                         ND_OFFICE_REGISTRY_PATH,
                                                         wow6432: false);
        }

        /// <summary>
        /// Gets the number of registry properties for ndOffice the sub key by the given the registry path.
        /// </summary>
        /// <returns>The number of registry properties for the sub key or null if it's not in the registry.</returns>
        public static int? GetNumberOfPropertiesForNdOfficeSubKey(RegistryHive registryHive)
        {
            return RegistryHelper.GetNumberOfPropertiesForSubkey(registryHive,
                                                                 ND_OFFICE_REGISTRY_PATH,
                                                                 wow6432: false);
        }

        /// <summary>
        /// Gets the value associated with the specified property name for ndOffice registry settings.
        /// If the name is not found, returns the default value.
        /// </summary>
        /// <param name="registryHive">Registry hive to get value.</param>
        /// <param name="propertyName">Property name. This string is not case-sensitive.</param>
        /// <returns>The value associated with the specified property name, or default value if the name is not found.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the given propertyName is not valid ndOffice registry property.</exception>
        public static T GetRegistryValue<T>(RegistryHive registryHive, string propertyName)
        {
            EnsureNdOfficePropertyIsValid(propertyName);

            return RegistryHelper.GetRegistryValue<T>(registryHive,
                                                      ND_OFFICE_REGISTRY_PATH,
                                                      propertyName,
                                                      wow6432: false);
        }

        /// <summary>
        /// Sets the value of a name/value pair in the registry key, using the specified
        /// registry data type.
        /// </summary>
        /// <param name="registryHive">Registry hive to get value.</param>
        /// <param name="propertyName">Property name. This string is not case-sensitive.</param>
        /// <param name="value">Property value.</param>
        /// <exception cref="System.ArgumentException">Thrown when the given propertyName is not valid ndOffice registry property.</exception>
        public static void SetRegistryValue<T>(RegistryHive registryHive,
                                               string propertyName,
                                               T value,
                                               bool createValueIfNotExists = false)
        {
            EnsureNdOfficePropertyIsValid(propertyName);

            RegistryHelper.SetRegistryValue(registryHive,
                                            ND_OFFICE_REGISTRY_PATH,
                                            propertyName,
                                            value,
                                            wow6432: false,
                                            createValueIfNotExists: createValueIfNotExists);
        }

        /// <summary>
        /// Determines if the given propertyName is the proper ndOffice registry property name.
        /// </summary>
        /// <returns>True if the given propertyName is the proper ndOffice registry property; otherwise false.</returns>
        public static bool IsNdOfficeProperty(string propertyName)
        {
            return RegistryPropertiesNames.Contains(propertyName);
        }

        /// <summary>
        /// Sets the value of a name/value pair in the registry key, using the specified
        /// registry data type, or creates a new name/value pair in the registry key if it doesn't exist.
        /// </summary>
        /// <param name="registryHive">Registry hive to get value.</param>
        /// <param name="propertyName">Property name. This string is not case-sensitive.</param>
        /// <param name="value">Property value.</param>
        /// <exception cref="System.ArgumentException">Thrown when the given propertyName is not valid ndOffice registry property.</exception>
        public static void SetOrCreateIfNotExists<T>(RegistryHive registryHive,
                                                     string propertyName,
                                                     T value)
        {
            EnsureNdOfficePropertyIsValid(propertyName);

            try
            {
                if (IsNdOfficeRegistrySubkeyExists(registryHive))
                {
                    SetRegistryValue(registryHive, propertyName, value, createValueIfNotExists: true);
                }
                else
                {
                    RegistryHelper.CreateSubKey(registryHive,
                                                ND_OFFICE_REGISTRY_PATH,
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

        /// <summary>
        /// Deletes the specified sub key and any child sub keys recursively.
        /// </summary>
        /// <param name="registryHive">Registry hive of the registry key.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        public static void DeleteNdOfficeSubKeyTree(RegistryHive registryHive, bool wow6432 = false)
        {
            if (RegistryHelper.IsRegistrySubkeyExists(registryHive, NETVOYAGE_REGISTRY_TREE_PATH, wow6432))
            {
                try
                { 
                    RegistryHelper.DeleteSubKeyTree(registryHive,
                                                    NETVOYAGE_REGISTRY_TREE_PATH,
                                                    wow6432);
                }
                catch (ArgumentException ex)
                {
                    // Note: ArgumentException here could only appear here because of missing registry key.
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Deletes all NdOffice sub keys and any child sub keys recursively from both HKCU and HKLM.
        /// </summary>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        public static void DeleteNdOfficeSubKeysTrees(bool wow6432 = false)
        {
            DeleteNdOfficeSubKeyTree(RegistryHive.CurrentUser);
            DeleteNdOfficeSubKeyTree(RegistryHive.LocalMachine);
        }

        /// <summary>
        /// Deletes value for ndOffice key property.
        /// </summary>
        /// <param name="registryHive">Registry hive to delete value.</param>
        /// <param name="propertyName">Property name. This string is not case-sensitive.</param>
        public static void DeleteNdOfficeSubKeyProperty(RegistryHive registryHive, string propertyName)
        {
            EnsureNdOfficePropertyIsValid(propertyName);

            RegistryHelper.DeleteRegistryValue(registryHive,
                                               ND_OFFICE_REGISTRY_PATH,
                                               propertyName,
                                               wow6432: false);
        }

        private static void EnsureNdOfficePropertyIsValid(string propertyName)
        {
            if (!IsNdOfficeProperty(propertyName))
            {
                throw new ArgumentException($"Given property: <{propertyName}> is not valid ndOffice registry property.");
            }
        }
    }
}
