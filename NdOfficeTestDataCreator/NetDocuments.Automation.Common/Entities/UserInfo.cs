namespace NetDocuments.Automation.Common.Entities
{
    /// <summary>
    /// Holds information about user credentials.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// An user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// An user password.
        /// </summary>
        public string Password { get; set; }

        public UserInfo (string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }
    }
}