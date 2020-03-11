using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NetDocuments.Rest.Contracts.Models.V1;

using NetDocuments.Automation.Helpers;
using NetDocuments.Automation.Helpers.Entities;
using NetDocuments.Automation.RestClient;

using NetDocuments.Automation.TestDataManagement.Abstract;
using NetDocuments.Automation.TestDataManagement.Entities;

namespace NetDocuments.Automation.TestDataManagement.Implementation
{
    /// <summary>
    /// Class for operate with documents on Nd web server.
    /// </summary>
    public class NdRestRepository : INdWebRepository
    {
        private readonly NdRestApiFacade restApi;
        private readonly string repositoryId;
        private readonly string cabinetId;
        private readonly (string nodeId, string nodeName)[] rootPath;
        private List<FolderEntity> internalFoldersStructure;

        public IFoldersEntity FoldersStructure { get; }

        /// <summary>
        /// Creates a new instance of the NdRestRepository class.
        /// </summary>
        /// <param name="restApi">NdRestApiFacade object.</param>
        /// <param name="repositoryId">Repository id.</param>
        /// <param name="cabinetId">Cabinet id with admin rights.</param>
        /// <param name="foldersEntity">Folders structure to create.</param>
        public NdRestRepository(NdRestApiFacade restApi, string repositoryId, string cabinetId, IFoldersEntity foldersEntity)
        {
            this.restApi = restApi;
            this.repositoryId = repositoryId;
            this.cabinetId = cabinetId;
            FoldersStructure = foldersEntity;

            var repositoryInfo = restApi.GetRepositoryInfo(repositoryId)
                ?? throw new InvalidOperationException($"Could not get info for {repositoryId} repository.");

            var cabinetInfo = restApi.GetCabinetInfo(cabinetId)
                ?? throw new InvalidOperationException($"Could not get info for {cabinetId} cabinet.");

            rootPath = new[]
            {
                (repositoryInfo.Id, repositoryInfo.Name),
                (cabinetInfo.Id, cabinetInfo.Name),
                ($"{repositoryInfo.Id}>{cabinetInfo.Id}>Folders", "Folder Folders")
            };

            internalFoldersStructure = CreateFoldersStructure(cabinetId, rootPath);
        }

        public WebDocumentInfo CreateDocument(string folderName, string docType, string sourceFileName, NdDocumentAttribute[] profileAttributes)
        {
            var folder = GetFolderByName(string.IsNullOrEmpty(folderName) ? FoldersStructure.Some : folderName);
            var docName = Path.GetRandomFileName();

            var document = restApi.CreateNewDocument(folder.Id, $"{docName}.{docType}", sourceFileName, profileAttributes)
                ?? throw new InvalidOperationException($"Could not create document in {folder.Id}");

            var id = document.DocInfo?.Id
                ?? throw new InvalidOperationException("Document was not created or created with errors.");

            var documentFullInfo = restApi.GetDocumentInfo(id, getVersions: true)
                ?? throw new InvalidOperationException($"Could not get info for document {id}");

            if (documentFullInfo.Versions?.Count != 1)
            {
                throw new InvalidOperationException($"Document {document.DocInfo.Id} was created without any or with more then 1 version.");
            }

            var documentPath = restApi.GetDocumentPath(folder.Id);
            var fullDocumentPath = rootPath.Concat(documentPath);
            fullDocumentPath = fullDocumentPath.Concat(new[] { (folder.Id, folder.Name) });

            return new WebDocumentInfo
            {
                Id = id,
                Envelope = documentFullInfo.DocInfo.Envelope,
                Name = documentFullInfo.DocInfo.Name,
                Path = fullDocumentPath.ToArray(),
                Attributes = profileAttributes?.ToList(),
                CheckedIn = documentFullInfo.CheckedOut == null,
                Versions = documentFullInfo.Versions
                                           .Select(v => new WebVersionInfo
                                           {
                                               IsApproved = false,
                                               IsLocked = v.IsLocked,
                                               IsOfficial = v.IsOfficial,
                                               Number = v.Number,
                                               VersionLabel = v.VersionLabel,
                                               VersionType = v.Extension,
                                               Description = v.Description
                                           })
                                           .ToList()
            };
        }

        public FolderEntity CreateFolder(string name, string parentContainerId)
        {
            var newFolderInfo = restApi.CreateFolder(name, parentContainerId, cabinetId);
            var folder = new FolderEntity
            {
                Id = newFolderInfo.Envelope,
                Name = name,
                Path = rootPath.Concat(new[] { (newFolderInfo.Envelope, name) }).ToArray()
            };
            internalFoldersStructure.Add(folder);
            return folder;
        }

        public WebVersionInfo CreateVersion(string documentId, string docType, int sourceVersion, string sourceFileName, bool isOfficial, string description)
        {
            var newVersionInfo = restApi.CreateDocumentVersion(documentId,
                                                               docType,
                                                               sourceVersion,
                                                               sourceFileName,
                                                               description,
                                                               versionName: string.Empty,
                                                               isOfficial: isOfficial,
                                                               addToRecent: false);

            if (newVersionInfo?.Extension != docType
                && newVersionInfo?.IsOfficial != isOfficial
                && newVersionInfo?.Description != description)
            {
                throw new InvalidOperationException($"Cannot create a version for document Id: {documentId}.");
            }

            return new WebVersionInfo
            {
                Number = newVersionInfo.Number,
                VersionLabel = newVersionInfo.VersionLabel,
                VersionType = newVersionInfo.Extension,
                IsOfficial = newVersionInfo.IsOfficial,
                Description = newVersionInfo.Description
            };
        }

        public void EditVersionContent(string documentId, int number, string sourceFileName)
            => restApi.EditVersionContent(documentId, number, sourceFileName, DateTime.Now);

        public void DeleteDocument(string id)
        {
            restApi.CheckIn(id);
            restApi.DeleteDocument(id);
        }

        public void DeleteFolder(string name, bool recursive)
        {
            var folder = GetFolderByName(name);

            if (folder != null)
            {
                restApi.DeleteFolder(folder.Id, recursive);
            }
        }

        public void DeleteVersion(string documentId, int number)
            => restApi.DeleteDocumentVersion(documentId, number);

        /// <summary>
        /// Cleans all remote resources.
        /// </summary>
        public void Dispose()
        {
            //Wait.ForResult(() => Parallel.ForEach(internalFoldersStructure,
            //                                      folder => restApi.DeleteFolder(id: folder.Id, deleteContent: true, isForceDelete: true))
            //                             .IsCompleted);
            restApi.Dispose();
        }

        private List<FolderEntity> CreateFoldersStructure(string cabinetId, (string nodeId, string nodeName)[] path)
        {
            var cabinetFolders = restApi.GetCabinetFolders(cabinetId);

            return FoldersStructure.FoldersNames.Select(name =>
            {
                var folderInfo = Array.Find(cabinetFolders, folder => folder.Name == name)
                    ?? restApi.CreateFolder(name, null, cabinetId);

                return new FolderEntity
                {
                    Id = folderInfo.Envelope,
                    Name = name,
                    Path = path.Concat(new[] { (folderInfo.Envelope, name) }).ToArray()
                };
            }).ToList();
        }

        private FolderEntity GetFolderByName(string name)
            => internalFoldersStructure.Find(f => f.Name == name);
    }
}