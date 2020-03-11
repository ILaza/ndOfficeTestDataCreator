using System.Collections.Generic;

namespace NetDocuments.Automation.Common.Entities
{
    /// <summary>
    /// Holds information for login.
    /// </summary>
    public class LoginTestData
    {
        // TODO: user depended property. We need to move it inside UserInfo entity.
        /// <summary>
        /// User identifier.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Refresh token.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Title for Login dialog.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Dictionary of users for login,
        /// where key is UserType string and value is UserInfo object.
        /// </summary>
        public Dictionary<string, UserInfo> Users { get; set; }

        /// <summary>
        /// The confirmation message text.
        /// </summary>
        public string ConfirmationMessage { get; set; }
    }
}