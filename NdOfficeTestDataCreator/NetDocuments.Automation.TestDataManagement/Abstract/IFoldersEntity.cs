namespace NetDocuments.Automation.TestDataManagement.Abstract
{
    /// <summary>
    /// Represents a folder structure in a repository
    /// </summary>
    public interface IFoldersEntity
    {
        /// <summary>
        /// Should be implemented to return some valid name in a repository.
        /// </summary>
        string Some { get; }

        /// <summary>
        /// Should be implemented to return some valid folder name in a repository.
        /// which satisfies the condition: Some != Another.
        /// </summary>
        string Another { get; }

        /// <summary>
        /// Should be implemented to return an array of folder names in a repository.
        /// </summary>
        string[] FoldersNames { get; }
    }
}
