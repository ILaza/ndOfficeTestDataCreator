using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

using NetDocuments.Rest.Contracts.Enums;

namespace NetDocuments.Automation.RestClient.Infrastructure
{
    /// <summary>
    /// Represents access token info.
    /// </summary>
    public class TokenInfo
    {
        private Lazy<SecureString> tokenSecureString = new Lazy<SecureString>(() => new SecureString());

        /// <summary>
        /// Gets or sets access token expiration date.
        /// </summary>
        public DateTime? TokenExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets token type.
        /// </summary>
        public TokenTypeHint TokenType { get; set; }

        /// <summary>
        /// Gets or sets access token.
        /// </summary>
        public string Token
        {
            get
            {
                IntPtr valuePtr = IntPtr.Zero;
                try
                {
                    valuePtr = Marshal.SecureStringToGlobalAllocUnicode(tokenSecureString.Value);
                    return Marshal.PtrToStringUni(valuePtr);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                }
            }
            set
            {
                if (tokenSecureString.IsValueCreated)
                {
                    tokenSecureString.Value.Clear();
                }

                value?.ToList().ForEach(i => tokenSecureString.Value.AppendChar(i));
            }
        }
    }
}
