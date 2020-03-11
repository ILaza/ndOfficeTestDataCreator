using Microsoft.Win32;
using System;
using System.IO;
using System.Management.Automation;
using System.Text;

using NetDocuments.Automation.Common.Exceptions;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Helper to activate ndOffice addins if needed.
    /// </summary>
    public static class AddinActivationHelper
    {
        private const string OTLOOK_RESILIENCY_REG_PATH = @"Software\Microsoft\Office\{0}.0\Outlook\Resiliency";
        private const string DO_NOT_DISABLE_ADDIN_LIST_KEY_NAME = "DoNotDisableAddinList";
        private const string DO_NOT_DISABLE_ADDIN_LIST_REG_PATH = @"Software\Microsoft\Office\{0}.0\Outlook\Resiliency\DoNotDisableAddinList";
        
        // Note: 0x00000001 Boot load (LoadBehavior = 3). https://support.microsoft.com/en-us/kb/2758876
        private const int BOOT_LOAD_NOT_DISABLED_ADDIN_VALUE = 1;

        // Note: means always load add-in on office app start. https://msdn.microsoft.com/en-us/library/bb386106.aspx
        private const int SOFT_ENABLE_ADDIN_VALUE = 3;

        // Note: means always load add-in
        private const string POLICY_ENABLE_ADDIN_VALUE = "1";

        private const string SOFT_DISABLED_ADDINS_REG_PATH = @"Software\Microsoft\Office\{0}\Addins\{1}";
        private const string SOFT_DISABLED_ADDINS_KEY = "LoadBehavior";
        private const int SOFT_ENABLED_ADDIN_STATE = 3;

        private const string ADDIN_NAME_FORMATTER = "NetDocuments.Client.{0}AddIn";
        private const string HARD_DISABLED_ADDINS_PATH = @"Software\Microsoft\Office\{0}.0\{1}\Resiliency\DisabledItems";
        private const string STARTUP_DISABLED_ITEMS = @"Software\Microsoft\Office\{0}.0\{1}\Resiliency\StartupItems";
        private const string ADDIN_POLICIES_PATH = @"Software\Policies\Microsoft\Office\{0}.0\{1}\Resiliency\AddinList";
        private const string NET_DOCUMENTS_ADDIN_DISTINQUISH_MARK = "netdocuments.client";
        private static readonly int[] SUPPORTED_OFFICE_VERSIONS = new int[] { 14, 15, 16 };

        private static string GetSoftDisableRegPath(string appName)
        {
            string addInName = string.Format(ADDIN_NAME_FORMATTER, appName);
            return string.Format(SOFT_DISABLED_ADDINS_REG_PATH, appName, addInName);
        }

        private static void EnableSoftDisabledAddin(string appName)
        {
            if (!CheckSoftDisable(appName))
            {
                return;
            }

            string registryPath = GetSoftDisableRegPath(appName);

            RegistryHelper.SetRegistryValue(RegistryHive.CurrentUser,
                                            registryPath,
                                            SOFT_DISABLED_ADDINS_KEY,
                                            SOFT_ENABLED_ADDIN_STATE,
                                            wow6432: false);
        }

        private static void AddAddinToDoNotDisableAddinListForOutlook(int officeVersion)
        {
            var resilencySubKey = string.Format(OTLOOK_RESILIENCY_REG_PATH, officeVersion);

            if (!RegistryHelper.IsRegistrySubkeyExists(RegistryHive.CurrentUser, resilencySubKey, wow6432: false))
            {
                return;
            }

            var regSubKey = string.Format(DO_NOT_DISABLE_ADDIN_LIST_REG_PATH, officeVersion);

            if (!RegistryHelper.IsRegistrySubkeyExists(RegistryHive.CurrentUser, regSubKey, wow6432: false))
            {
                if (!RegistryHelper.HasSubKeyCreated(RegistryHive.CurrentUser, resilencySubKey, DO_NOT_DISABLE_ADDIN_LIST_KEY_NAME))
                {
                    return;
                }
            }

            var outlookAddInName = string.Format(ADDIN_NAME_FORMATTER, "Outlook");
            var currentHKCUValue = RegistryHelper.GetRegistryValue<int?>(RegistryHive.CurrentUser,
                                                                         regSubKey,
                                                                         outlookAddInName,
                                                                         wow6432: false);

            // Note: if add-in is already in the list "DoNotDisableAddinList" and value is not BOOT_LOAD_NOT_DISABLED_ADDIN_VALUE we set it.
            if (currentHKCUValue.HasValue
                && currentHKCUValue.Value != BOOT_LOAD_NOT_DISABLED_ADDIN_VALUE)
            {
                RegistryHelper.SetRegistryValue(RegistryHive.CurrentUser,
                                                regSubKey,
                                                outlookAddInName,
                                                BOOT_LOAD_NOT_DISABLED_ADDIN_VALUE,
                                                wow6432: false);
            }

            // Note: if add-in is absent in the list, we create value and set BOOT_LOAD_NOT_DISABLED_ADDIN_VALUE.
            else if (!currentHKCUValue.HasValue)
            {
                RegistryHelper.CreateSubKey(RegistryHive.CurrentUser, regSubKey, outlookAddInName, BOOT_LOAD_NOT_DISABLED_ADDIN_VALUE);
            }
        }

        /// <summary>
        /// Checks the soft disable.
        /// </summary>
        /// <param name="appName">The office application name.</param>
        /// <returns>Returns true if disabled.</returns>
        private static bool CheckSoftDisable(string appName)
        {
            string registryPath = GetSoftDisableRegPath(appName);

            var currentHKCUValue = RegistryHelper.GetRegistryValue<int?>(RegistryHive.CurrentUser,
                                                                         registryPath,
                                                                         SOFT_DISABLED_ADDINS_KEY,
                                                                         wow6432: false);

            return currentHKCUValue.HasValue && currentHKCUValue != SOFT_ENABLE_ADDIN_VALUE;
        }

        /// <summary>
        /// Requests addins state and pushes messages about current state (is it can be determined).
        /// </summary>
        /// <param name="appName">The office application name.</param>
        /// <returns>Returns true if disable add-in exist.</returns>
        public static bool CheckCurrentAddinsState(string appName)
        {
            try
            {
                var versionFromRegistry = MSOfficeRegistryHelper.GetMsOfficeVersionFromRegistry()
                    ?? throw new AdditionalCheckFailureException("Cannot get MS Office version from registry.");

                var isParsed = Int32.TryParse(versionFromRegistry.Split('.')[2], out int version);

                if (!isParsed)
                {
                    throw new ParseException("Cannot parse MS Office version.");
                }

                var resiliencyStartupItemsPath = string.Format(STARTUP_DISABLED_ITEMS, version, appName);
                var isResiliencyStartupItemsExist = RegistryHelper.IsRegistrySubkeyExists(RegistryHive.CurrentUser,
                                                                                          resiliencyStartupItemsPath,
                                                                                          wow6432: false);

                if (isResiliencyStartupItemsExist)
                {
                    RegistryHelper.DeleteSubKeyTree(RegistryHive.CurrentUser, resiliencyStartupItemsPath);
                }

                bool? isDisabledAddinExist = null;

                bool? result = IsAddinForApplicationEnabled(appName, version);

                if (result.HasValue && !result.Value && !isDisabledAddinExist.HasValue)
                {
                    isDisabledAddinExist = !result;
                }

                if (isDisabledAddinExist.HasValue)
                {
                    return isDisabledAddinExist.Value;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex} was thrown during check current addin state.");
                return false;
            }
        }

        private static bool? IsAddinForApplicationEnabled(string appName, int version)
        {
            bool? isPolicySetsToEnabled = CheckPolicyValueForApplication(appName, version);

            if (isPolicySetsToEnabled.HasValue)
            {
                return isPolicySetsToEnabled;
            }

            bool isHardDisabled = CheckHardDisabledAddinForApplication(appName, version);
            bool isSoftDisabled = CheckSoftDisabledAddinForApplication(appName);

            if (isHardDisabled)
            {
                return false;
            }
            else
            {
                if (isSoftDisabled)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private static bool CheckSoftDisabledAddinForApplication(string appName)
        {
            bool areDisabledAddinExist = false;
            try
            {
                bool isAddinDisabled = CheckSoftDisable(appName);

                if (isAddinDisabled)
                {
                    areDisabledAddinExist = true;
                }

                return areDisabledAddinExist;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex} was thrown during check soft disabled addin.");
                return false;
            }
        }

        private static bool CheckHardDisabledAddinForApplication(string appName, int version)
        {
            bool areDisabledAddinsExist = false;
            var disabledValue = GetHardDisabledAddinsValue(version, appName);

            if (disabledValue != null)
            {
                areDisabledAddinsExist = true;
            }

            return areDisabledAddinsExist;
        }

        private static bool? CheckPolicyValueForApplication(string appName, int version)
        {
            bool? policyCheckResult = null;

            string addInName = string.Format(ADDIN_NAME_FORMATTER, appName);

            var policyRegistryPath = string.Format(ADDIN_POLICIES_PATH, version, appName);

            var currentHKCUValue = RegistryHelper.GetRegistryValue<string>(RegistryHive.CurrentUser,
                                                                           policyRegistryPath,
                                                                           addInName,
                                                                           wow6432: false);

            if (currentHKCUValue != null)
            {
                if (currentHKCUValue != POLICY_ENABLE_ADDIN_VALUE)
                {
                    return false;
                }

                policyCheckResult = true;
            }

            return policyCheckResult;
        }

        /// <summary>
        /// Removes registry value for ndOffice disabled addin. Removing it from DisabledItems means it is enabled.
        /// </summary>
        /// <param name="appName">The office application name.</param>
        public static void ActivateAddin(string appName)
        {
            try
            {
                EnableHardDisabledAddin(appName);
                EnableSoftDisabledAddin(appName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex} was thrown during activate addin.");
            }
        }

        private static void EnableHardDisabledAddin(string appName)
        {
            foreach (var officeVersion in SUPPORTED_OFFICE_VERSIONS)
            {
                GetHardDisabledAddinsValue(officeVersion, appName, removeValue: true);

                if (appName == "Outlook")
                {
                    AddAddinToDoNotDisableAddinListForOutlook(officeVersion);
                }
            }
        }

        /// <summary>
        /// Checks if for office application ndOffice addin is disabled and returns registry value of disabled addin.
        /// </summary>
        /// <param name="officeVersion">Office version.</param>
        /// <param name="appName">The office application name.</param>
        /// <param name="removeValue">Remove disabled addin value or not.</param>
        /// <returns>Registry value of disabled ndOffice addin. null if doesn't exist.</returns>
        private static string GetHardDisabledAddinsValue(int officeVersion, string appName, bool removeValue = false)
        {
            var officeRegistryPath = string.Format(HARD_DISABLED_ADDINS_PATH, officeVersion, appName);

            string disabledAddinRegistryValue = null;
            RegistryKey key = null;

            try
            {
                key = Registry.CurrentUser.OpenSubKey(officeRegistryPath, true);

                // Note: means that disabled items for such application don't exist or application isn't installed.
                if (key == null)
                {
                    return disabledAddinRegistryValue;
                }

                var disabledNames = key.GetValueNames();
                foreach (var disabledValueName in disabledNames)
                {
                    var valueKind = key.GetValueKind(disabledValueName);

                    if (valueKind == RegistryValueKind.Binary)
                    {
                        var regValue = (byte[])key.GetValue(disabledValueName);
                        int pathLength = 0;
                        using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(regValue)))
                        {
                            binaryReader.ReadInt32();

                            // Note: the next four bytes are the length of the path.
                            pathLength = binaryReader.ReadInt32();
                            binaryReader.Close();
                        }

                        if (regValue.Length >= 12 + pathLength)
                        {
                            string addinPath = Encoding.Unicode.GetString(regValue, 12, pathLength - 2);

                            if (addinPath.ToLower().Contains(NET_DOCUMENTS_ADDIN_DISTINQUISH_MARK))
                            {
                                if (removeValue)
                                {
                                    key.DeleteValue(disabledValueName);
                                }
                                else
                                {
                                    disabledAddinRegistryValue = disabledValueName;
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex} was thrown during get hard disabled addin value.");
            }
            finally
            {
                key?.Close();
            }
            return disabledAddinRegistryValue;
        }
    }
}
