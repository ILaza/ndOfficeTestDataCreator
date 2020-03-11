namespace NetDocuments.Automation.Common.Entities.Outlook
{
    /// <summary>
    /// Class which keep EMCloud error messages.
    /// </summary>
    public static class EMCloudErrorMesseges
    {
        /// <summary>
        /// Error message which informs that selected item is larger than 1GB.
        /// </summary>
        public const string ItemMoreThanOneGB = "This item is larger than 1GB.Please use ndOffice to save the message separately from the attachments.";

        /// <summary>
        /// Error message which informs that login is required.
        /// </summary>
        public const string LoginIsRequired = "Please log in to see filing predictions and file email.";

        /// <summary>
        /// Error message which informs that there are no predicted filing locations.
        /// </summary>
        public const string NoDataToGeneratePredictions = "There are no predicted filing locations.Please browse to find a filing location.";
    }
}
