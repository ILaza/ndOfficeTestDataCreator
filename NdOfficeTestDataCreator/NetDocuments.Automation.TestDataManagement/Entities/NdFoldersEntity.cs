using System;
using System.Reflection;

using NetDocuments.Automation.TestDataManagement.Abstract;

namespace NetDocuments.Automation.TestDataManagement.Entities
{
    public class NdFoldersEntity : IFoldersEntity
    {
        /// <inheritdoc cref="IFoldersEntity" />
        public string PerformanceTestFolder { get; }

        /// <inheritdoc cref="IFoldersEntity" />
        public string[] FoldersNames => new[] { PerformanceTestFolder };

        public NdFoldersEntity()
        {
            var now = DateTime.Now;

            // Note: In order for this to work versioning on CI should be configured
            var version = Assembly.GetExecutingAssembly()
                                  .GetName()
                                  .Version
                                  .ToString();

            PerformanceTestFolder = $"Test Folder {now.ToString("yyyy_MM_dd")}_{version}";
        }
    }
}
