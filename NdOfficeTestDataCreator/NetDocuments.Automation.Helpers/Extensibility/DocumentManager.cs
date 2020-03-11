using NetDocuments.Automation.Common.Settings;
using NetDocuments.Automation.Helpers.Entities;
using NetDocuments.Client.COM;
using NetDocuments.Client.COM.Interfaces;
using System;
using System.IO;

namespace NetDocuments.Automation.Helpers.Extensibility
{
    public class DocumentManager
    {
        public static void AddDocumentToACWithExtensibility(string documentId, int documentversion, bool addToRecents, HostSettings NdHostSettings)
        {
                using (var authContextHelper = new AuthorizationContextHelper().ResolveService())
                {
                    authContextHelper.Login(NdHostSettings.ClientId, NdHostSettings.SecretId);

                    using (var extensibilityHelper = new ExtensibilityService2Helper().ResolveService(authContextHelper.GetAuthorizationContext()))
                    {
                        var docPath = extensibilityHelper.CheckOut(documentId,
                                                                   documentversion,
                                                                   addToRecents: addToRecents);

                        extensibilityHelper.CheckIn(docPath, uploadChanges: false, addToRecents: true);
                    }
                }
            }

        ////public static void CheckInDocumentWithExtensibility(string documentName, bool uploadChanges, bool addToRecents, HostSettings NdHostSettings)
        ////{
        ////    int checkInOperationResult;

        ////    using (var authorizationHelper = new AuthorizationContextHelper().ResolveService())
        ////    {
        ////        authorizationHelper.Login(NdHostSettings.ClientId, NdHostSettings.SecretId);

        ////        using (var extensibilityHelper = new ExtensibilityService2Helper().ResolveService(authorizationHelper.GetAuthorizationContext()))
        ////        {
        ////            var documentPath = GetDocumentEchoPath(documentName);
        ////            checkInOperationResult = extensibilityHelper.CheckIn(documentPath, uploadChanges, addToRecents);
        ////        }
        ////    }

        ////    if (checkInOperationResult != 0)
        ////    {
        ////        //throw new ExtensibilityMethodFailedException("Could not check in document.");
        ////    }
        ////}

        ////public static string GetDocumentEchoPath(string docName)
        ////{
        ////    var EchoFolderPath = Path.Combine(@"C:\Users\", Environment.UserName, NdOfficeConstants.ECHO_FOLDER_NAME);
        ////    return CheckOutHelper.WaitForDocument(EchoFolderPath, docName);
        ////}
    }
}
