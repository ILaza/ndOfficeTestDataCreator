using System;
using NetDocuments.Automation.Common.Exceptions.Extensibility;
using NetDocuments.Client.COM;
using NetDocuments.Client.COM.Interfaces;

namespace NetDocuments.Automation.Helpers.Extensibility
{
    public class ExtensibilityService2Helper : IDisposable
    {
        private IExtensibilityService2 extensibilityService2;
        private COMObjectsHelper comHelper;

        /// <summary>
        /// Resolves extensibility COM object and creates new instance of ExtensibilityService2Helper.
        /// </summary>
        /// <returns>ExtensibilityService2Helper object instance.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when login into extensibility failed with any error.</exception>
        public ExtensibilityService2Helper ResolveService(IAuthorizationContext authorizationContext)
        {
            comHelper = new COMObjectsHelper();

            if (extensibilityService2 == null)
            {
                extensibilityService2 = comHelper.Register(() => (IExtensibilityService2)TypeHelper.GetInstance(InterfacesIds.NETDOCUMENTS_EXTENSIBILITY_SERVICE_2_PROG_ID));

                extensibilityService2.SetAuthorizationContext(authorizationContext);
            }
            return this;
        }

        public string CheckOut(string documentId, int documentVersion,  bool addToRecents)
        {
            extensibilityService2.Checkout2(documentId, documentVersion, raiseEvent: false, addToRecents: addToRecents);
            Wait.ForResult(() => extensibilityService2.CheckCheckOut2Result().ResultReady);
            var checkOut2Result = extensibilityService2.CheckCheckOut2Result();
            CheckExtensibilityResult((ExtensibilityResult)checkOut2Result.Result);

            return checkOut2Result.FilePath;
        }

        public int CheckIn(string documentPath, bool uploadChanges, bool addToRecents)
        {
            extensibilityService2.Checkin2(documentPath, uploadChanges, raiseEvent: false, addToRecents: addToRecents);
            Wait.ForResult(() => extensibilityService2.CheckCheckIn2Result().ResultReady);
            var checkIn2Result = extensibilityService2.CheckCheckIn2Result();
            CheckExtensibilityResult((ExtensibilityResult)checkIn2Result.Result);

            return checkIn2Result.Result;
        }

        public void Dispose()
        {
            comHelper.Dispose();
        }

        private void CheckExtensibilityResult(ExtensibilityResult result)
        {
            if (result != ExtensibilityResult.Successful)
            {
                throw new ExtensibilityMethodFailedException($"Result from extensibility service hasn't been successful: actual result {result.ToString()}.");
            }
        }
    }
}
