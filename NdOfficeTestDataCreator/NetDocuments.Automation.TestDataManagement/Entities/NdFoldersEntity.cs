using System;
using System.Reflection;

using NetDocuments.Automation.TestDataManagement.Abstract;

namespace NetDocuments.Automation.TestDataManagement.Entities
{
    public class NdFoldersEntity : IFoldersEntity
    {
        /// <inheritdoc cref="IFoldersEntity" />
        public string Some { get; }

        /// <inheritdoc cref="IFoldersEntity" />
        public string Another { get; }

        /// <inheritdoc cref="IFoldersEntity" />
        public string[] FoldersNames => new[] { Some, Another };

        public NdFoldersEntity()
        {
            var now = DateTime.Now;

            // Note: In order for this to work versioning on CI should be configured
            var version = Assembly.GetExecutingAssembly()
                                  .GetName()
                                  .Version
                                  .ToString();

            Some = $"Some_{now.ToString("yyyy_MM_dd")}_{version}";
            Another = $"Another_{now.ToString("yyyy_MM_dd")}_{version}";
        }
    }
}
