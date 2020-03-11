using System;
using System.Linq;
using System.Security;

using Microsoft.Win32;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Holds method to work with registry.
    /// </summary>
    public static class RegistryHelper
    {
        /// <summary>
        /// Gets the value associated with the specified property name.
        /// If the name is not found, returns the default value if getDefault is true
        /// or null if getDefault is false.
        /// </summary>
        /// <param name="registryHive">Registry hive to get value.</param>
        /// <param name="baseRegistryPath">Base path to the registry key.</param>
        /// <param name="propertyName">Property name. This string is not case-sensitive.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        /// <param name="getDefault">Flag to set if you need a default value.</param>
        /// <returns>The value associated with the specified property name.</returns>
        public static T GetRegistryValue<T>(RegistryHive registryHive, string baseRegistryPath,
                                            string propertyName, bool wow6432, bool getDefault = false)
        {
            var value = default(T);
            using (var baseKey = GetBaseKey(registryHive, wow6432))
            {
                using (var applicationSubKey = baseKey.OpenSubKey(baseRegistryPath))
                {
                    if (applicationSubKey != null)
                    {
                        if (getDefault)
                        {
                            value = (T)applicationSubKey.GetValue(propertyName, default(T));
                        }
                        else
                        {
                            value = (T)applicationSubKey.GetValue(propertyName);
                        }
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Sets value to special key property.
        /// </summary>
        /// <param name="registryHive">Registry hive to set value.</param>
        /// <param name="baseRegistryPath">Base path to the registry key.</param>
        /// <param name="propertyName">Property name. This string is not case-sensitive.</param>
        /// <param name="value">Property value.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        /// <param name="createValueIfNotExists">Determines whether value should be created if not exists.</param>
        public static void SetRegistryValue<T>(RegistryHive registryHive,
                                               string baseRegistryPath,
                                               string propertyName,
                                               T value,
                                               bool wow6432,
                                               bool createValueIfNotExists = false)
        {
            using (var baseKey = GetBaseKey(registryHive, wow6432))
            {
                using (var applicationSubKey = baseKey.OpenSubKey(baseRegistryPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (applicationSubKey == null)
                    {
                        return;
                    }

                    var isValueExists = applicationSubKey.GetValueNames()
                                                         .Any(i => string.Equals(i, propertyName, StringComparison.OrdinalIgnoreCase));

                    if (createValueIfNotExists && !isValueExists)
                    {
                        applicationSubKey.SetValue(propertyName, value);
                        return;
                    }

                    var registryValueKind = applicationSubKey.GetValueKind(propertyName);
                    applicationSubKey.SetValue(propertyName, value, registryValueKind);
                }
            }
        }

        /// <summary>
        /// Gets base registry key.
        /// <remarks>
        /// Do not forget to dispose a returned value.
        /// </remarks>
        /// </summary>
        /// <returns>Base registry key.</returns>
        public static RegistryKey GetBaseKey(RegistryHive registryHive, bool wow6432)
        {
            return RegistryKey.OpenBaseKey(registryHive,
                                           Environment.Is64BitOperatingSystem && !wow6432
                                               ? RegistryView.Registry64
                                               : RegistryView.Registry32);
        }

        /// <summary>
        /// Determines whether registry key can be accessed.
        /// </summary>
        /// <param name="registryHive">Registry hive to get the value.</param>
        /// <param name="registryPath">Path to the key.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        public static bool CanAccessKeyPath(RegistryHive registryHive,
                                            string registryPath,
                                            RegistryKeyPermissionCheck registryKeyPermissionCheck,
                                            bool wow6432 = false)
        {
            try
            {
                using (var baseKey = GetBaseKey(registryHive, wow6432))
                {
                    using (var applicationSubKey = baseKey.OpenSubKey(registryPath, registryKeyPermissionCheck))
                    {
                        return true;
                    }
                }
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if registry sub key exists for the given path.
        /// </summary>
        /// <param name="registryHive">Registry hive to get value.</param>
        /// <param name="baseRegistryPath">Base path to the registry key.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        /// <returns>True if the registry sub key exists; otherwise false.</returns>
        public static bool IsRegistrySubkeyExists(RegistryHive registryHive,
                                                  string baseRegistryPath,
                                                  bool wow6432)
        {
            using (var baseKey = GetBaseKey(registryHive, wow6432))
            {
                using (var applicationSubKey = baseKey.OpenSubKey(baseRegistryPath))
                {
                    return applicationSubKey != null;
                }
            }
        }

        /// <summary>
        /// Gets the number of registry properties for the sub key given the registry path.
        /// </summary>
        /// <param name="registryHive">Registry hive to get value.</param>
        /// <param name="baseRegistryPath">Base path to the registry key.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        /// <returns>The number of registry properties for the sub key or null if it's not in the registry.</returns>
        public static int? GetNumberOfPropertiesForSubkey(RegistryHive registryHive,
                                                          string baseRegistryPath,
                                                          bool wow6432)
        {
            using (var baseKey = GetBaseKey(registryHive, wow6432))
            {
                using (var applicationSubKey = baseKey.OpenSubKey(baseRegistryPath))
                {
                    return applicationSubKey?.ValueCount;
                }
            }
        }

        /// <summary>
        /// Saves setting into property.
        /// <remarks>
        /// Could be saved only User properties.
        /// </remarks>
        /// </summary>
        /// <param name="registryHive">Registry hive to get value.</param>
        /// <param name="baseRegistryPath">Base path to the registry key.</param>
        /// <param name="propertyName">Property name. This string is not case-sensitive.</param>
        /// <param name="value">Property value.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        public static void CreateSubKey<T>(RegistryHive registryHive,
                                           string baseRegistryPath,
                                           string propertyName,
                                           T value,
                                           bool wow6432 = false)
        {
            using (var baseKey = GetBaseKey(registryHive, wow6432))
            {
                using (var applicationSubKey = baseKey.CreateSubKey(baseRegistryPath))
                {
                    if (applicationSubKey == null)
                    {
                        throw new InvalidOperationException($"Cannot create: <{baseRegistryPath}> sub key in the registry.");
                    }
                    applicationSubKey.SetValue(propertyName, value);
                }
            }
        }

        /// <summary>
        /// Creates a registry sub key.
        /// </summary>
        /// <param name="registryHive">Registry hive.</param>
        /// <param name="baseRegistryPath">Base path to key.</param>
        /// <param name="subKeyName">Key name.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        /// <returns>True if sub key was created.</returns>
        public static bool HasSubKeyCreated(RegistryHive registryHive,
                                            string baseRegistryPath,
                                            string subKeyName,
                                            bool wow6432 = false)
        {
            using (var baseKey = GetBaseKey(registryHive, wow6432))
            {
                using (var applicationSubKey = baseKey.CreateSubKey(baseRegistryPath))
                {
                    if (applicationSubKey == null)
                    {
                        throw new InvalidOperationException($"Cannot create: <{baseRegistryPath}> sub key in the registry.");
                    }

                    using (var newKey = applicationSubKey.CreateSubKey(subKeyName))
                    {
                        return newKey != null;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the specified sub key and any child sub keys recursively.
        /// </summary>
        /// <param name="registryHive">Registry hive of the registry key.</param>
        /// <param name="baseRegistryPath">Base path to the registry key.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        /// <param name="throwOnMissingSubKey">Throw if key is missing from the registry.</param>
        public static void DeleteSubKeyTree(RegistryHive registryHive, string baseRegistryPath,
                                            bool wow6432 = false, bool throwOnMissingSubKey = true)
        {
            using (var baseKey = GetBaseKey(registryHive, wow6432))
            {
                baseKey.DeleteSubKeyTree(baseRegistryPath, throwOnMissingSubKey);
            }
        }

        /// <summary>
        /// Deletes value for special key property.
        /// </summary>
        /// <param name="registryHive">Registry hive to delete value.</param>
        /// <param name="baseRegistryPath">Base path to the registry key.</param>
        /// <param name="propertyName">Property name. This string is not case-sensitive.</param>
        /// <param name="wow6432">Do you want to access 32-bit key.</param>
        /// <param name="throwOnMissingValue">Indicates whether an exception should be raised if the specified value cannot be found.</param>
        public static void DeleteRegistryValue(RegistryHive registryHive,
                                               string baseRegistryPath,
                                               string propertyName,
                                               bool wow6432,
                                               bool throwOnMissingValue = false)
        {
            using (var baseKey = GetBaseKey(registryHive, wow6432))
            {
                using (var applicationSubKey = baseKey.OpenSubKey(baseRegistryPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (applicationSubKey == null)
                    {
                        return;
                    }

                    applicationSubKey.DeleteValue(propertyName, throwOnMissingValue);
                }
            }
        }
    }
}
