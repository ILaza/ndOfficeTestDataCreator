using System;

using NetDocuments.Client.COM;
using NetDocuments.Client.COM.Interfaces;

namespace NetDocuments.Automation.Helpers.Extensibility
{
    public class ExtensibilityInformationHelper : IDisposable
    {
        private IExtensibilityInformation extensibilityInformation;
        private COMObjectsHelper comHelper;

        /// <summary>
        /// Resolves extensibility COM object and creates new instance of ExtensibilityInformationHelper.
        /// </summary>
        /// <returns>ExtensibilityInformationHelper object instance.</returns>
        public ExtensibilityInformationHelper ResolveService()
        {
            comHelper = new COMObjectsHelper();

            if (extensibilityInformation == null)
            {
                extensibilityInformation = comHelper.Register(() => (IExtensibilityInformation)TypeHelper.GetInstance(InterfacesIds.NETDOCUMENTS_EXTENSIBILITY_EXTENSIBILITYINFORMATION_PROG_ID));
            }
            return this;
        }

        public string GetNdOfficeVersion()
        {
            return extensibilityInformation.NdOfficeVersion;
        }

        public void Dispose()
        {
            comHelper.Dispose();
        }
    }
}
