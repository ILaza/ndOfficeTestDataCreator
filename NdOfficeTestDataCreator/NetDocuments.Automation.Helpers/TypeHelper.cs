using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Helper class to work with COM objects.
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// Creates and returns an instance of the type associated with the specified progId.
        /// Returns null if an error is encountered while loading the System.Type.
        /// </summary>
        /// <param name="progId">The ProgID of the type to get.</param>
        /// <returns>
        /// An instance of the type associated with the specified progId or null
        /// if an error is encountered while loading the System.Type.
        /// </returns>
        public static object GetInstance(string progId)
        {
            var isPresent = Wait.ForResult(() => RegistryHelper.IsRegistrySubkeyExists(RegistryHive.ClassesRoot,
                                                                                       $@"{progId}\CLSID",
                                                                                       wow6432: !Environment.Is64BitProcess));

            if (!isPresent)
            {
                throw new InvalidComObjectException($"Type with id: {progId} is not found in the registry and cannot be instanciated.");
            }

            var oType = Type.GetTypeFromProgID(progId);

            return Activator.CreateInstance(oType);
        }

        /// <summary>
        /// Releases all references to a COM object.
        /// </summary>
        /// <param name="value">Dynamic object to release.</param>
        public static void ReleaseCOMObject(object value)
        {
            if (value != null && Marshal.IsComObject(value))
            {
                Marshal.FinalReleaseComObject(value);
            }
        }
    }
}