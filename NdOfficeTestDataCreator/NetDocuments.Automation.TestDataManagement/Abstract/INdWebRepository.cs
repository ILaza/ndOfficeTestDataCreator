using System;

using NetDocuments.Automation.Helpers.Entities;
using NetDocuments.Rest.Contracts.Models.V1;

using NetDocuments.Automation.TestDataManagement.Abstract;
using NetDocuments.Automation.TestDataManagement.Entities;

namespace NetDocuments.Automation.TestDataManagement.Implementation
{
    /// <summary>
    /// Represents interface for document repository.
    /// </summary>
    public interface INdWebRepository : IDisposable
    {
        IFoldersEntity FoldersStructure { get; }

        WebDocumentInfo CreateDocument(string folderName, string docType, string sourceFileName, NdDocumentAttribute[] profileAttributes);

        WebVersionInfo CreateVersion(string documentId, string docType, int sourceVersion, string sourceFileName, bool isOfficial, string description);

        void EditVersionContent(string documentId, int number, string sourceFileName);

        FolderEntity CreateFolder(string name, string parentContainerId);

        void DeleteDocument(string id);

        void DeleteVersion(string documentId, int number);

        void DeleteFolder(string name, bool recursive);
    }
}
