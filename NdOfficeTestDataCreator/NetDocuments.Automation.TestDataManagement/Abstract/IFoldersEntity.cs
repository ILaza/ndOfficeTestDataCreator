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
        string PerformanceTestFolder { get; }

        /// <summary>
        /// Should be implemented to return an array of folder names in a repository.
        /// </summary>
        string[] FoldersNames { get; }
    }
}
