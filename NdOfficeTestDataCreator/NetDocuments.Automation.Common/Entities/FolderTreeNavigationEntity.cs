using System.Collections.Generic;

namespace NetDocuments.Automation.Common.Entities
{
    /// <summary>
    /// Holds testing data for navigation in folder tree.
    /// </summary>
    public class FolderTreeNavigationEntity
    {
        /// <summary>
        /// Tree Dictionary where key is a node name and value is NodeDataEntity object.
        /// </summary>
        public Dictionary<string, NodeDataEntity> Tree { get; set; }
    }
}
