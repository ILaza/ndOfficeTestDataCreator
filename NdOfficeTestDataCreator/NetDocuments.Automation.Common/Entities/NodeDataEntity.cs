namespace NetDocuments.Automation.Common.Entities
{
    /// <summary>
    /// Holds node test data information.
    /// </summary>
    public class NodeDataEntity
    {
        /// <summary>
        /// Holds navigation path for different folders.
        /// </summary>
        public (string nodeId, string nodeName)[] NavigationPath { get; set; }

        /// <summary>
        /// Holds root navigation node name.
        /// </summary>
        public string NavigationRoot { get; set; }

        /// <summary>
        /// Holds full location name in formats:
        /// ".../{folder name}" in case it is a root folder.
        /// ".../{parent folder name}/{folder name}" in case it is not a root folder.
        /// </summary>
        public string FullLocationName { get; set; }
    }
}
