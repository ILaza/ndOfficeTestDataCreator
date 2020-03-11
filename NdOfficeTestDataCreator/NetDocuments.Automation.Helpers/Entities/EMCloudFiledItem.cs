namespace NetDocuments.Automation.Helpers.Entities
{
    /// <summary>
    /// Hols information about filed item.
    /// </summary>
    public class EMCloudFiledItem
    {
        /// <summary>
        /// Date of filing process. 
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Action name.
        /// </summary>
        public string Action { get; set; }
        
        /// <summary>
        /// Name of filed item.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// EnvelopeID of filed item.
        /// </summary>
        public string EnvelopeId { get; set; }

        /// <summary>
        /// ID of filed item.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Location ID.
        /// </summary>
        public string FileToLocationId { get; set; }

        /// <summary>
        /// Item sent date.
        /// </summary>
        public string SentDate { get; set; }

        /// <summary>
        /// Outlook item message ID.
        /// </summary>
        public string MessageId { get; set; }
    }
}
