namespace NetDocuments.Automation.Helpers.Entities
{
    /// <summary>
    /// Class for holding data related to profile attribute.
    /// </summary>
    public class ProfileAttributeEntity
    {
        /// <summary>
        /// Gets or sets profile Attribute identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets profile Attribute name.
        /// </summary>
        public string Name { get; set; }

        public string Value { get; set; }

        public string RepositoryId { get; set; }
    }
}
