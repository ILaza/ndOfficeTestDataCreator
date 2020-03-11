using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Contains helper methods to interact with Windows OS
    /// </summary>
    public static class WindowsSystemHelper
    {
        private static readonly Lazy<string> OSInfoInstance = new Lazy<string>(GetOsInfo);

        private static readonly Lazy<string> executablePath =
            new Lazy<string>(() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        /// <summary>
        /// Determines whether the specified user is an administrator.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the specified user is an administrator; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// WARNING: Users on the machines for running automation tests should
        /// have permisions for domain to which they belong.
        /// </remarks>
        /// <seealso href="https://ayende.com/blog/158401/are-you-an-administrator"/>
        public static bool IsUserAnAdmin()
        {
            PrincipalContext context;

            try
            {
                Domain.GetComputerDomain();

                try
                {
                    context = new PrincipalContext(ContextType.Domain);
                }
                catch (PrincipalServerDownException)
                {
                    context = new PrincipalContext(ContextType.Machine);
                }
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                context = new PrincipalContext(ContextType.Machine);
            }

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                var user = UserPrincipal.FindByIdentity(context, identity.Name);

                if (user != null)
                {
                    return user.GetAuthorizationGroups()
                               .Any(principal => principal.Sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid)
                                    || principal.Sid.IsWellKnown(WellKnownSidType.AccountDomainAdminsSid)
                                    || principal.Sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid)
                                    || principal.Sid.IsWellKnown(WellKnownSidType.AccountEnterpriseAdminsSid));
                }
                return false;
            }
        }

        /// <summary>
        /// Gets general operating system info.
        /// </summary>
        /// <returns>OS info: name, version and architecture.</returns>
        public static string OsInfo => OSInfoInstance.Value;

        /// <summary>
        /// Gets full path to current application executable directory.
        /// </summary>
        /// <returns>Full path to current application executable directory.</returns>
        public static string ExecutableDir => executablePath.Value;

        /// <summary>
        /// Determines the machine type from the given assembly/executable.
        /// </summary>
        /// <param name="fileName">Path to the file.</param>
        /// <returns>Machine type.</returns>
        public static string GetBitness(string fileName)
        {
            const int PE_POINTER_OFFSET = 60;
            const int MACHINE_OFFSET = 4;
            byte[] data = new byte[4096];

            using (Stream s = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                s.Read(data, 0, 4096);
            }

            // Dos header is 64 bytes, last element, long (4 bytes) is the address of the PE header
            int PE_HEADER_ADDR = BitConverter.ToInt32(data, PE_POINTER_OFFSET);
            int machineUint = BitConverter.ToUInt16(data, PE_HEADER_ADDR + MACHINE_OFFSET);
            return ((MachineType)machineUint).ToString();
        }

        private enum MachineType
        {
            Unknown = 0,
            x86 = 0x014c,
            i64 = 0x0200,
            x64 = 0x8664
        }

        /// <summary>
        /// Gets general operating system info.
        /// </summary>
        /// <returns>OS info: name, version and architecture.</returns>
        private static string GetOsInfo()
        {
            var machineName = Environment.MachineName;
            var osName = GetOsName(Environment.OSVersion);
            var osVersion = Environment.OSVersion.Version.ToString();
            var osArchString = Environment.Is64BitOperatingSystem
                ? "x64"
                : "x32";

            return $"{machineName}/{osName} {osArchString} {osVersion}";
        }

        private static string GetOsName(OperatingSystem osInfo)
        {
            var version = $"{osInfo.Version.Major.ToString()}.{osInfo.Version.Minor.ToString()}";
            switch (version)
            {
                case "10.0": return "Windows 10";
                case "6.3": return "Windows 8.1/Server 2012 R2";
                case "6.2": return "Windows 8/Windows Server 2012";
                case "6.1": return "Windows 7/Windows Server 2008 R2";
                case "6.0": return "Windows Server 2008/Windows Vista";
                case "5.2": return "Windows Server 2003 R2/Windows Server 2003/Windows XP 64-Bit Edition";
                case "5.1": return "Windows XP";
                case "5.0": return "Windows 2000";
            }
            return "Unknown";
        }
    }
}