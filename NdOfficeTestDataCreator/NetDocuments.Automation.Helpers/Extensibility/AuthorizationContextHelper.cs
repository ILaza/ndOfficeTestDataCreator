using System;

using NetDocuments.Automation.Common.Exceptions.Extensibility;
using NetDocuments.Client.COM;
using NetDocuments.Client.COM.Interfaces;

namespace NetDocuments.Automation.Helpers.Extensibility
{
    /// <summary>
    /// Helper class for operate with extensibility service object.
    /// </summary>
    public class AuthorizationContextHelper : IDisposable
    {
        private static IAuthorizationContext extensibilityServiceObject;

        /// <summary>
        /// Determines whether user logged in into ndOffice.
        /// </summary>
        public bool IsUserLoggedIn => extensibilityServiceObject.NdOfficeUser != null;

        /// <summary>
        /// Gets host name for ndOffice settings.
        /// </summary>
        public string CurrentHostName => extensibilityServiceObject.CurrentHostName;

        /// <summary>
        /// Gets user name of current NdOffice user.
        /// </summary>
        public string CurrentNdOfficeUser =>
            IsUserLoggedIn
                ? extensibilityServiceObject.NdOfficeUser.Split(' ')[0].ToLower()
                : string.Empty;

        /// <summary>
        /// Initializes new instance of the ExtensibilityAuthorizationContextHelper.
        /// </summary>
        public AuthorizationContextHelper()
        {
        }

        /// <summary>
        /// Gets COM object instance.
        /// </summary>
        /// <returns>COM object instance.</returns>
        public AuthorizationContextHelper ResolveService()
        {
            if (extensibilityServiceObject == null)
            {
                extensibilityServiceObject = (IAuthorizationContext)TypeHelper.GetInstance(InterfacesIds.NETDOCUMENTS_EXTENSIBILITY_AUTHORIZATIONCONTEXT_PROG_ID);
            }
            return this;
        }

        public IAuthorizationContext GetAuthorizationContext()
            => extensibilityServiceObject;

        public void Login(string clientId, string clientSecret, string title = "Login into Extensibility")
        {
            extensibilityServiceObject.Login(title, clientId, clientSecret, raiseEvent: false);

            var isLoggedIn = Wait.ForResult(() => extensibilityServiceObject.CheckResult().ResultReady);

            if (!isLoggedIn)
            {
                throw new ExtensibilityLoginFailedException("Failed to login using extensibility.");
            }
        }

        /// <summary>
        /// Frees all resources.
        /// </summary>
        public void Dispose()
        {
            TypeHelper.ReleaseCOMObject(extensibilityServiceObject);
            extensibilityServiceObject = null;
        }
    }
}
