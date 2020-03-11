using System.Management.Automation;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Holds method to work with PowerShell.
    /// </summary>
    public static class PowerShellHelper
    {
        /// <summary>
        /// Execute given script on PowerShell.
        /// </summary>
        /// <param name="script">Script that must be executed.</param>
        public static void ExecutePowerShellScript(string script)
        {
            using (var PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript(script);
                PowerShellInstance.Invoke();
            }
        }
    }
}