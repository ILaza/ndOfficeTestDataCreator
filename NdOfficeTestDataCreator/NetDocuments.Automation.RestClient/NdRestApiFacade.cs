using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using NetDocuments.Rest.Contracts.Controllers.V1;
using NetDocuments.Rest.Contracts.Controllers.V2;
using NetDocuments.Rest.Contracts.Enums;
using NetDocuments.Rest.Contracts.Enums.V1;
using NetDocuments.Rest.Contracts.Models;
using NetDocuments.Rest.Contracts.Models.V1;

using NetDocuments.Automation.Common.Exceptions.Server;
using NetDocuments.Automation.Helpers;
using NetDocuments.Automation.Helpers.Entities;
using NetDocuments.Automation.RestClient.Infrastructure;

namespace NetDocuments.Automation.RestClient
{
    public class NdRestApiFacade : IDisposable
    {
        private const int TIMEOUT_MILLISECONDS = 120000;
        private const int RETRY_RATE_DELAY_MILLI_SECONDS = 10000;
        private const int NUMBER_OF_MINUTES_TO_UPDATE_DOCUMENT_INFO = 1;

        private bool disposedValue = false;

        private readonly IV1Facade v1Facade;
        private readonly IV2Facade v2Facade;
        private readonly CustomRestClient customRestClient;
        private readonly OAuthFacade oAuthFacade;

        public NdRestApiFacade(IV1Facade v1Facade,
                               IV2Facade v2Facade,
                               CustomRestClient customRestClient,
                               OAuthFacade oAuthFacade)
        {
            this.v1Facade = v1Facade;
            this.v2Facade = v2Facade;
            this.customRestClient = customRestClient;
            this.oAuthFacade = oAuthFacade;
        }

        /// <summary>
        /// Gets the last version number for the given document.
        /// </summary>
        /// <param name="documentId">Document id.</param>
        /// <returns>Returns last document version for the given document.</returns>
        public int GetLastDocumentVersionNumber(string documentId)
        {
            var result = SendRequest(() => v1Facade.Document.GetVersionsList(documentId),
                                     timeoutMilliSeconds: 10000,
                                     retryRateDelayMilliSeconds: 2000);

            if (result == null)
            {
                throw new RestRequestFailedException($"Can't get the list of versions for the document {documentId} from REST.");
            }
            else if (result.Items == null || result.Items?.Any() != true)
            {
                throw new RestRequestFailedException($"Version items collection is empty.");
            }

            return result.Items.Max(i => i.Number);
        }

        /// <summary>
        /// Deletes all non official versions of the given document.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <returns>True if all items were deleted successfully; otherwise false.</returns>
        public bool DeleteNotOfficialVersions(string id)
        {
            var items = SendRequest(() => v1Facade.Document.GetVersionsList(id))?.Items;

            if (items != null)
            {
                return items.Where(version => !version.IsOfficial && !version.IsLocked)
                            .Select(version => v1Facade.Document.DeleteDocumentVersion(id, version.Number).IsSuccessful)
                            .ToList()
                            .All(i => i);
            }

            return false;
        }

        /// <summary>
        /// Checks if the document exists on the server.
        /// </summary>
        /// <param name="documentId">Document id.</param>
        /// <returns>True, if document exists on the server; otherwise false.</returns>
        /// TODO: Check if a document isn't marked as deleted.
        public bool DocumentExists(string documentId)
            => SendRequest(() => v1Facade.Document.GetDocumentInfo(documentId)) != null;

        /// <summary>
        /// Gets the NdDocumentInformation object by the document id.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <param name="getVersions">Boolean value; default is "false". If value is true 
        /// the returned object will include a "docVersions" array in the another way - won't include.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The NdDocumentInformation object by the document id.</returns>
        public NdDocumentInformation GetDocumentInfo(string documentId, bool getVersions = false, CancellationToken token = default)
            => SendRequest(() => v1Facade.Document.GetDocumentInfo(documentId, getVersions: getVersions, token: token, useLongName: true));

        /// <summary>
        /// Deletes the document version.
        /// </summary>
        /// <param name="documentId">Document id.</param>
        /// <param name="versionNumber">Version number.</param>
        public void DeleteDocumentVersion(string documentId, int versionNumber)
        {
            var operationResult = v1Facade.Document.DeleteDocumentVersion(documentId, versionNumber);

            if (!operationResult.IsSuccessful)
            {
                throw new RestRequestFailedException($"Could not delete the version for the document id: {documentId} and the version number: {versionNumber}.");
            }
        }

        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <returns>True if the document was deleted successfully; otherwise false.</returns>
        /// TODO: Probably we can use response status or code from the delete call to reduce the number of rest calls here.
        public bool DeleteDocument(string documentId)
        {
            if (DocumentExists(documentId))
            {
                return v1Facade.Document.DeleteDocument(documentId).IsSuccessful;
            }

            return true;
        }

        /// <summary>
        /// Gets the number of document versions by the document id.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The number of document versions.</returns>
        public int GetDocumentVersionsNumber(string documentId, CancellationToken token = default)
        {
            var versionsList = SendRequest(() => v1Facade.Document.GetVersionsList(documentId, token))
                ?? throw new RestRequestFailedException($"Cannot get versions list for the document with Id: {documentId}.");

            return versionsList.Items.Count;
        }

        /// <summary>
        /// Gets the list of WebVersionInfo by the document id.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The list of WebVersionInfo.</returns>
        public List<WebVersionInfo> GetDocumentWebVersionInfos(string documentId, CancellationToken token = default)
        {
            var versionsList = SendRequest(() => v1Facade.Document.GetVersionsList(documentId, token));

            return versionsList?.Items
                                .Select(i => new WebVersionInfo
                                {
                                    Number = i.Number,
                                    VersionLabel = i.VersionLabel,
                                    VersionType = i.Extension,
                                    IsOfficial = i.IsOfficial,
                                    Description = i.Description
                                })
                                .OrderBy(_ => _.Number)
                                .ToList();
        }

        /// <summary>
        /// Gets modified date for the given document id.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <returns>Document modified date.</returns>
        public DateTime GetDocumentModifiedDate(string documentId)
        {
            var documentInfo = GetDocumentInfo(documentId)?.DocInfo
                ?? throw new RestRequestFailedException($"Cannot get document information for the document id: {documentId}.");

            return documentInfo.ModifiedDate;
        }

        /// <summary>
        /// Waits until the document will be modified.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <param name="latestModifiedDate">The latest DateTime when the document was modified.</param>
        public void WaitUntilDocumentWillBeModified(string documentId, DateTime latestModifiedDate)
            => Wait.For(() => latestModifiedDate.CompareTo(GetDocumentModifiedDate(documentId)) < 0,
                        TIMEOUT_MILLISECONDS, retryRateDelayMilliSeconds: RETRY_RATE_DELAY_MILLI_SECONDS);

        /// <summary>
        /// Gets the document version description.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <param name="versionNumber">Version number.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Description of the document version.</returns>
        public string GetDocumentVersionDescription(string documentId, int versionNumber)
        {
            // TODO: We can get the specific version info instead of all versions.
            var requestResult = SendRequest(() => v1Facade.Document.GetVersionsList(documentId))
                ?? throw new RestRequestFailedException($"Cannot get versions list for the document with Id: {documentId}.");

            var documentVersion = requestResult.Items.Find(version => version.Number == versionNumber)
                ?? throw new NotFoundException($"Cannot find the version: {versionNumber} in the document with Id: {documentId}.");

            return documentVersion.Description;
        }

        /// <summary>
        /// Updates the document version description and gets Nd document statistics as a result.
        /// </summary>
        /// <param name="envelopeId">The document envelope id.</param>
        /// <param name="versionNumber">Version number.</param>
        /// <param name="description">Version description in JSON format.</param>
        /// <returns>Nd document statistics.</returns>
        public NdDocumentStat UpdateVersionDescription(string envelopeId, int versionNumber, string description)
            => SendRequest(() => v1Facade.Document.UpdateVersionInfo(envelopeId, versionNumber, description));

        /// <summary>
        /// Saves the document's official version content to a file by the given path.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="fullFilePath">Full file path.</param>
        /// TODO: Investigate if we can use 'DownloadVersionContent' instead.
        public void DownloadOfficialVersionContent(string id, string fullFilePath)
        {
            var docInfo = GetDocumentInfo(id, true)
                ?? throw new RestRequestFailedException($"Cannot get information for the document with Id: {id}.");

            var officialVersion = docInfo.DocInfo.OfficialVersion;

            using (var documentStream = new MemoryStream())
            {
                SendRequest(() => v1Facade.Document.ReadDocument(id, documentStream, checkout: false, addToRecent: false, version: officialVersion));
                // TODO: Use a static method from 'File' class.
                var fsHelper = new FileSystemHelper();
                fsHelper.CreateFileFromStream(documentStream, fullFilePath);
            }
        }

        /// <summary>
        /// Saves the document's version content to file by the given path.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="versionNumber">Document version number.</param>
        /// <param name="fullFilePath">Full file path.</param>
        public void DownloadVersionContent(string id, int versionNumber, string fullFilePath)
        {
            var versionExists = GetDocumentInfo(id, true)?.Versions?.Find(v => v.Number == versionNumber) != null;

            if (versionExists)
            {
                // TODO: Use FileStream instead of MemoryStream. Investigate a case when file is 2 GB in size.
                using (var documentStream = new MemoryStream())
                {
                    SendRequest(() => v1Facade.Document.ReadDocument(id, documentStream, checkout: false, addToRecent: false, version: versionNumber));
                    var fsHelper = new FileSystemHelper();
                    fsHelper.CreateFileFromStream(documentStream, fullFilePath);
                }
            }
        }

        /// <summary>
        /// Checks if the document exists in the folder.
        /// </summary>
        /// <param name="documentName">Document name.</param>
        /// <param name="folderId">Folder Id.</param>
        /// <returns>True, if the document exists in the folder; otherwise false.</returns>
        /// TODO: Investigate a better way to do this check based on document locations.
        public bool IsDocumentExistInFolder(string folderId, string documentName)
        {
            var info = SendRequest(() => v1Facade.Folder.GetFolderContent(folderId,
                                                                          foldersOnly: false,
                                                                          documentTypeFilter: null,
                                                                          profileAttributeIdsToLoad: null,
                                                                          skipToken: null,
                                                                          token: default,
                                                                          allResults: true),
                                   timeoutMilliSeconds: 5000,
                                   retryRateDelayMilliSeconds: 1000)
                ?? throw new RestRequestFailedException($"Cannot get content for the folder with Id: {folderId}.");

            return info.NdContentItemWithAttributes.Any(item => item.StandardAttributes
                                                                    .Name
                                                                    .Equals(documentName));
        }

        /// <summary>
        /// Sets the official document version to a given.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="version">Document version.</param>
        public void SetOfficialVersionTo(string id, int version)
        {
            SendRequest(() => v1Facade.Document.SetOfficialVersion(id, version));
        }

        /// <summary>
        /// Creates the new version of a document from the official version.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="extension">Version extension.</param>
        /// <param name="description">Version description.</param>
        /// <param name="versionName">Version name.</param>
        public NdDocumentStat CreateNewVersionFromOfficial(string id,
                                                           string extension,
                                                           string description,
                                                           string versionName,
                                                           bool isOfficial = false,
                                                           bool addToRecent = false,
                                                           bool checkOut = false)
            => SendRequest(() => v1Facade.Document.CreateDocumentVersionFromOfficialVersion(id,
                                                                                            extension,
                                                                                            description,
                                                                                            versionName,
                                                                                            isOfficial,
                                                                                            isSubVersion: false,
                                                                                            addToRecent,
                                                                                            checkOutRequired: CheckOutState.ShouldNotBeCheckedOutByAnyone,
                                                                                            checkOut));

        /// <summary>
        /// Gets child profile attributes for a given parent profile attribute.
        /// </summary>
        /// <param name="repositoryId">Repository id.</param>
        /// <param name="attributeId">Profile attribute id.</param>
        /// <param name="parentValue">Parent profile attribute value.</param>
        /// <returns>List of child profile attributes linked to a specific parent on the server.</returns>
        public NdLookupList GetChildProfileAttributes(string repositoryId, int attributeId, string parentValue)
            => SendRequest(() => v1Facade.CustomAttributeValues.GetLookup(repositoryId, attributeId, parentValue));

        /// <summary>
        /// Gets a profile attribute value for the document.
        /// </summary>
        /// <param name="documentId">Document id.</param>
        /// <param name="profileAttributeId">Profile attribute id.</param>
        public NdDocumentAttribute GetProfileAttributeForDocument(string documentId, int profileAttributeId)
        {
            var docInfo = GetDocumentInfo(documentId)
                ?? throw new RestRequestFailedException($"Cannot get information for the document with Id: {documentId}.");

            return docInfo.Attributes.Find(attribute => attribute.Id == profileAttributeId);
        }

        /// <summary>
        /// Gets cabinet profile attributes for a cabinet with a given id.
        /// </summary>
        /// <param name="cabinetId">Cabinet id.</param>
        /// <returns>Collection of NdCustomAttrInfo.</returns>
        public NdCustomAttrInfo[] GetCabinetProfileAttributes(string cabinetId)
            => SendRequest(() => v1Facade.Cabinet.GetCabinetAttributes(cabinetId));

        /// <summary>
        /// Gets the version info.
        /// </summary>
        /// <param name="documetId">Document id.</param>
        /// <param name="versionNumber">Version number.</param>
        /// <returns>Version info.</returns>
        public NdVersionInformation GetVersionInfo(string documetId, int versionNumber)
            => SendRequest(() => v1Facade.Document.GetVersionInfo(documetId, versionNumber));

        /// <summary>
        /// Gets the folder content.
        /// </summary>
        /// <param name="parentFolderId">Parent folder id.</param>
        /// <param name="foldersOnly">Gets folder only.</param>
        /// <param name="documentTypeFilter">Document types filter.</param>
        /// <param name="profileAttributeIdsToLoad">Collection of profile attributes to load.</param>
        /// <param name="skipToken">Skip token.</param>
        /// <param name="token">Cancellation token.</param>
        /// <param name="allResults">Get all results.</param>
        /// <returns>NdContentItemsListWithAttributes object.</returns>
        public NdContentItemsListWithAttributes GetFolderContent(string parentFolderId,
                                                                 bool foldersOnly,
                                                                 string documentTypeFilter,
                                                                 List<int> profileAttributeIdsToLoad,
                                                                 string skipToken = null,
                                                                 CancellationToken token = default,
                                                                 bool allResults = false)
            => SendRequest(() => v1Facade.Folder.GetFolderContent(parentFolderId,
                                                                  foldersOnly,
                                                                  documentTypeFilter,
                                                                  profileAttributeIdsToLoad,
                                                                  skipToken,
                                                                  token,
                                                                  allResults));

        /// <summary>
        /// Gets the user's recently opened documents.
        /// </summary>
        /// <param name="documentTypeFilter">Document types filter.</param>
        /// <param name="profileAttributeIdsToLoad">Collection of profile attributes ids to load.</param>
        /// <param name="bypassCache">Allows the caller to bypass the cache and get the most recent values.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>NdContentItemsListWithAttributes object.</returns>
        public NdContentItemsListWithAttributes GetUserRecentlyOpenedDocuments(string documentTypeFilter,
                                                                               List<int> profileAttributeIdsToLoad,
                                                                               bool bypassCache,
                                                                               CancellationToken token = default)
            => SendRequest(() => v1Facade.User.GetUserRecentlyOpenedDocuments(documentTypeFilter,
                                                                              profileAttributeIdsToLoad,
                                                                              bypassCache,
                                                                              token));

        /// <summary>
        /// Gets the user's recently edited documents.
        /// </summary>
        /// <param name="documentTypeFilter">Document types filter.</param>
        /// <param name="profileAttributeIdsToLoad">Collection of profile attributes ids to load.</param>
        /// <param name="bypassCache">Allows the caller to bypass the cache and get the most recent values.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>NdContentItemsListWithAttributes object.</returns>
        public NdContentItemsListWithAttributes GetUserRecentlyEditedDocuments(string documentTypeFilter,
                                                                               List<int> profileAttributeIdsToLoad,
                                                                               bool bypassCache,
                                                                               CancellationToken token = default)
            => SendRequest(() => v1Facade.User.GetUserRecentlyEditedDocuments(documentTypeFilter,
                                                                              profileAttributeIdsToLoad,
                                                                              bypassCache,
                                                                              token));

        /// <summary>
        /// Gets the user's recently added documents.
        /// </summary>
        /// <param name="documentTypeFilter">Document types filter.</param>
        /// <param name="profileAttributeIdsToLoad">Collection of profile attributes ids to load.</param>
        /// <param name="bypassCache">Allows the caller to bypass the cache and get the most recent values.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>NdContentItemsListWithAttributes object.</returns>
        public NdContentItemsListWithAttributes GetUserRecentlyAddedDocuments(string documentTypeFilter,
                                                                              List<int> profileAttributeIdsToLoad,
                                                                              bool bypassCache,
                                                                              CancellationToken token = default)
            => SendRequest(() => v1Facade.User.GetUserRecentlyAddedDocuments(documentTypeFilter,
                                                                             profileAttributeIdsToLoad,
                                                                             bypassCache,
                                                                             token));

        /// <summary>
        /// Gets the user's recent documents.
        /// </summary>
        /// <param name="documentTypeFilter">Document types filter.</param>
        /// <param name="profileAttributeIdsToLoad">ollection of profile attributes ids to load.</param>
        /// <param name="bypassCache">Allows the caller to bypass the cache and get the most recent values.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>NdContentItemsListWithAttributes object.</returns>
        public NdContentItemsListWithAttributes GetRecentDocuments(string documentTypeFilter,
                                                                   List<int> profileAttributeIdsToLoad,
                                                                   bool bypassCache,
                                                                   CancellationToken token = default)
            => SendRequest(() => v1Facade.User.GetUserRecentlyAccessedDocuments(documentTypeFilter,
                                                                                profileAttributeIdsToLoad,
                                                                                bypassCache,
                                                                                token));

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="id">Folder id.</param>
        /// <param name="deleteContent">If true delete the folder's content.</param>
        /// <param name="isForceDelete">If true then repository setting which restricts the ability for a deletion will be ignored.</param>
        /// <returns>Returns True if the delete operation is successful; False otherwise.</returns>
        public bool DeleteFolder(string id, bool deleteContent, bool isForceDelete = true)
            => SendRequest(() => v1Facade.Folder.DeleteFolder(id, deleteContent, isForceDelete));

        /// <summary>
        /// Addes the document to Recent document folder.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// TODO: Rewrite this method.
        public void AddDocumentToRecent(string id)
        {
            SendRequest(() => v1Facade.Document.Checkout(id, addToRecent: true));
            SendRequest(() => v1Facade.Document.Checkin(id));

            // Note: We use the explicit delay, because we have to wait until document
            // will be added to Recent Documents.
            Thread.Sleep(TimeSpan.FromMinutes(NUMBER_OF_MINUTES_TO_UPDATE_DOCUMENT_INFO));
        }

        /// <summary>
        /// Determines whether the document exists in workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace id.</param>
        /// <param name="documentTypeFilter">Documents type filter.</param>
        /// <param name="documentId">Document id.</param>
        /// <returns>True, if document with given id exists in workspace; otherwise false.</returns>
        /// TODO: Investigate if we can use 'Locations' or 'Ancetry' from v2 call DocumentInfo call.
        public bool IsDocumentExistsInWorkspace(string workspaceId, string documentTypeFilter, string documentId)
        {
            var workspaceContent = SendRequest(() => v1Facade.Workspace.GetWorkspaceDocuments(workspaceId,
                                                                                              documentTypeFilter,
                                                                                              skipToken: null,
                                                                                              profileAttributeIdsToLoad: null))
                ?? throw new RestRequestFailedException($"Cannot obtain documents from the workspace with Id: {workspaceId} and docTypeFilter: {documentTypeFilter}.");

            return workspaceContent.NdContentItemWithAttributes
                                   .Any(i => i.StandardAttributes.Id.Equals(documentId));
        }

        /// <summary>
        /// Creates the new official version from current official version and adds it to recent documents.
        /// </summary>
        /// <param name="id">Document id.</param>
        /// <param name="extension">Version extension.</param>
        /// <param name="description">Version description.</param>
        /// <param name="versionName">Version name.</param>
        /// <returns>New official version id.</returns>
        public int CreateNewOfficialVersionFromExisting(string id, string extension, string description, string versionName)
            => SendRequest(() => v1Facade.Document.CreateDocumentVersionFromOfficialVersion(id, extension,
                                                                                            description,
                                                                                            versionName,
                                                                                            isOfficial: true,
                                                                                            isSubVersion: false,
                                                                                            addToRecent: true,
                                                                                            checkOutRequired: CheckOutState.ShouldNotBeCheckedOutByAnyone,
                                                                                            checkOut: false))
                                         .OfficialVersion;

        /// <summary>
        /// Determines whether the document is filed at the cabinet level.
        /// </summary>
        /// <param name="documentId">Document id.</param>
        /// <returns>True if document has empty folder and workspace list; otherwise false.</returns>
        public bool HasDocumentFiledToTheCabinetLevel(string documentId)
        {
            // Note: for now we don't have any possibility to obtain level of filed document,
            // so we count folders and workspaces where this document is located.
            // If document is not located in any folder and workspace - it is located on a cabinet level.
            var documentLocations = GetDocumentLocations(documentId);
            return documentLocations?.FolderList?.Count == 0
                   && documentLocations?.WorkspaceList?.Count == 0;
        }

        /// <summary>
        /// Gets all ND document locations for a given document id.
        /// </summary>
        /// <param name="documentId">Document Id.</param>
        /// <returns>ND document locations; otherwise null.</returns>
        public NdDocumentLocations GetDocumentLocations(string documentId)
            => SendRequest(() => v1Facade.Document.GetDocumentLocations(documentId));

        /// <summary>
        /// Gets the security template from a cabinet with a given name for a given user.
        /// </summary>
        /// <param name="cabinetId">Cabinet id.</param>
        /// <param name="templateName">Template name.</param>
        /// <param name="userId">User Id.</param>
        /// <returns>Security template instance if exists; otherwise null.</returns>
        public NdSecurityTemplate GetSecurityTemplateForUser(string cabinetId, string templateName, string userId)
            => SendRequest(() => v1Facade.Cabinet.GetCabinetSecurityTemplates(cabinetId))
                                         ?.FirstOrDefault(t => t.Name.Equals(templateName)
                                                            && t.Permissions.All(p => p.User.Equals(userId)));

        /// <summary>
        /// Check-in the document.
        /// </summary>
        /// <param name="documentId">Document id.</param>
        /// <returns>True if check in was successful; otherwise false.</returns>
        public bool CheckIn(string documentId)
        {
            var documentInfo = GetDocumentInfo(documentId)
                ?? throw new RestRequestFailedException($"Could not get info for document {documentId}.");

            if (documentInfo.CheckedOut == null)
            {
                return true;
            }

            return SendRequest(() => v1Facade.Document.Checkin(documentId));
        }

        /// <summary>
        /// Creates the new official version for document.
        /// </summary>
        /// <param name="documentId">Document id.</param>
        /// <param name="extension">Document extension.</param>
        /// <param name="sourceFilePath">Document with content.</param>
        /// <param name="description">New version description.</param>
        /// <param name="versionName">Version name.</param>
        /// <param name="addToRecent">Add to recent.</param>
        /// <returns>True if new version was created; otherwise false.</returns>
        public bool CreateNewOfficialVersion(string documentId,
                                             string extension,
                                             string sourceFilePath,
                                             string description,
                                             string versionName,
                                             bool addToRecent = false)
        {
            var info = GetDocumentInfo(documentId);

            // TODO: Use errors from the REST instead of this check.
            if (info.CheckedOut != null || info.DocInfo == null)
            {
                // We can't create new version for checked out document.
                return false;
            }

            var docStatInfo = SendRequest(() => v1Facade.Document.CreateAndUploadDocumentVersion(documentId,
                                                                                                 extension,
                                                                                                 description,
                                                                                                 isOfficial: true,
                                                                                                 addToRecent: addToRecent,
                                                                                                 info.DocInfo.OfficialVersion,
                                                                                                 checkOut: false,
                                                                                                 sourceFilePath,
                                                                                                 sendProgress: false,
                                                                                                 contentServerSupported: false,
                                                                                                 isSubVersion: false,
                                                                                                 versionName: versionName));

            return docStatInfo?.OfficialVersion != info.DocInfo.OfficialVersion;
        }

        public NdVersionInformation CreateDocumentVersion(string documentId,
                                                          string extension,
                                                          int sourceVersion,
                                                          string sourceFilePath,
                                                          string description,
                                                          string versionName,
                                                          bool isOfficial = false,
                                                          bool addToRecent = false)
        {
            var stat = SendRequest(() => v1Facade.Document.CreateAndUploadDocumentVersion(documentId,
                                                                                          extension,
                                                                                          description,
                                                                                          isOfficial: isOfficial,
                                                                                          addToRecent: addToRecent,
                                                                                          sourceVersion,
                                                                                          checkOut: false,
                                                                                          sourceFilePath,
                                                                                          sendProgress: false,
                                                                                          contentServerSupported: false,
                                                                                          isSubVersion: false,
                                                                                          versionName: versionName));

            var newVersion = stat.NewVersion ?? throw new RestRequestFailedException($"Could not create new document version for document id: {documentId}.");

            return GetVersionInfo(documentId, newVersion);
        }

        /// <summary>
        /// Gets the list of document's versions.
        /// </summary>
        /// <param name="documentId">Document id.</param>
        /// <returns> NdVersionList if exists; otherwise null.</returns>
        public NdVersionList GetVersionsList(string documentId)
            => SendRequest(() => v1Facade.Document.GetVersionsList(documentId));

        /// <summary>
        /// Gets the cabinet profile templates.
        /// </summary>
        /// <param name="cabinetId">Cabinet ID.</param>
        /// <returns> Profile templates array; otherwise null.</returns>
        public NdProfileTemplate[] GetCabinetProfileTemplates(string cabinetId)
            => SendRequest(() => v1Facade.Cabinet.GetCabinetProfileTemplates(cabinetId));

        public NdDocumentInformation CreateNewDocument(string folderId,
                                                       string fileName,
                                                       string sourceFileName,
                                                       IEnumerable<NdDocumentAttribute> attributes)
        {
            try
            {
                var documentCreationInfo = SendRequest(() => v1Facade.Document.CreateDocument(directoryId: folderId,
                                                                                                isCabinetDestination: false,
                                                                                                fileName: fileName,
                                                                                                attributes: attributes,
                                                                                                permissions: null,
                                                                                                addToRecent: true,
                                                                                                checkOut: false))
                    ?? throw new RestRequestFailedException("Failed to create a document with Rest client.");

                var documentInfo = documentCreationInfo.DocInfo;

                var updateResult = SendRequest(() => v1Facade.Document.UpdateDocumentVersion(id: documentInfo.Id,
                                                                                                version: documentInfo.OfficialVersion,
                                                                                                sourceFileName: sourceFileName,
                                                                                                versionModifiedDate: documentInfo.ModifiedDate,
                                                                                                addToRecent: true,
                                                                                                persistLastModified: true,
                                                                                                checkOutRequired: false))
                    ?? throw new RestRequestFailedException($"Failed to create a version for document id: {documentInfo.Id} with Rest client.");

                return documentCreationInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public NdDocumentInformation CreateDocument(string folderId,
                                                    bool isCabinetDestination,
                                                    string fileName,
                                                    IEnumerable<NdDocumentAttribute> attributes)
        {
            try
            {
                var documentCreationInfo = SendRequest(() => v1Facade.Document.CreateDocument(directoryId: folderId,
                                                                                                isCabinetDestination: false,
                                                                                                fileName: fileName,
                                                                                                attributes: attributes,
                                                                                                permissions: null,
                                                                                                addToRecent: true,
                                                                                                checkOut: false))
                    ?? throw new RestRequestFailedException("Failed to create a document with Rest client.");

                return documentCreationInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public CabinetInfo[] GetUserCabinets()
            => SendRequest(() => v1Facade.User.GetUserCabinets())?.Select(ci =>
            {
                return new CabinetInfo
                {
                    Id = ci.Id,
                    LogoUri = ci.LogoUri,
                    Name = ci.Name,
                    RepositoryId = ci.RepositoryId,
                    RepositoryName = ci.RepositoryName,
                    WorkspaceAttributeID = ci.WorkspaceAttributeID,
                    WorkspaceAttributePluralName = ci.WorkspaceAttributePluralName,
                };
            })
            .ToArray();

        public (string nodeId, string nodeName)[] GetDocumentPath(string container)
        {
            var path = SendRequest(() => v1Facade.Folder.GetFolderParents(container));

            return path?.Select(p => (p.EnvId, p.Name)).ToArray();
        }

        public NdDocumentStat CreateFolder(string name, string parentContainerId, string cabinetId)
        {
            var info = SendRequest(() => v1Facade.Folder.CreateFolder(name, parentContainerId, cabinetId));

            return info?.DocInfo
                ?? throw new RestRequestFailedException($"Could not create folder {name} inside {parentContainerId} in cabinet {cabinetId}.");
        }

        public NdDocumentStat[] GetCabinetFolders(string cabinetId)
        {
            var info = SendRequest(() => v1Facade.Cabinet.GetCabinetFolders(cabinetId));

            return info?.Results?.ToArray() ?? throw new RestRequestFailedException($"Could not get folders for cabinet id: {cabinetId}.");
        }

        public NdLookupItem[] GetProfileAttributeLookupTable(string repositoryId, int attributeId)
            => SendRequest(() => v1Facade.CustomAttributeValues.GetLookup(repositoryId, attributeId))?.Items?.ToArray();

        public void EditVersionContent(string id,
                                       int number,
                                       string sourceFileName,
                                       DateTime versionModifiedDate,
                                       bool addToRecent = true,
                                       bool persistLastModified = true)
            => SendRequest(() => v1Facade.Document.UpdateDocumentVersion(id,
                                                                         number, 
                                                                         sourceFileName,
                                                                         versionModifiedDate, 
                                                                         addToRecent, 
                                                                         persistLastModified));

        public NdRepositoryInfo GetRepositoryInfo(string id)
            => SendRequest(() => v1Facade.Repository.GetRepositoryInfo(id));

        public NdCabinetInfo GetCabinetInfo(string id)
            => SendRequest(() => v1Facade.Cabinet.GetCabinetInfo(id));

        public NdDocumentStat[] GetRecentWorkspaces(string cabinetId)
        {
            return SendRequest(() => v1Facade.User.GetUserRecentWorkspaces(cabinetId))?.Results.ToArray();
        }

        public ProfileAttributeEntity GetWorkspaceParentAttribute(string cabinetId, int workspaceAttributeId)
        {
            var attributes = SendRequest(() => v1Facade.Cabinet.GetCabinetAttributes(cabinetId))
                ?? throw new RestRequestFailedException($"Cannot get attributes for the cabinet id: {cabinetId}.");

            var workspaceAttribute = attributes.FirstOrDefault(a => a.Id == workspaceAttributeId);

            var parent = attributes.FirstOrDefault(a => a.Id == workspaceAttribute?.Linked);
            
            if (parent == null)
            {
                return null;
            }

            return new ProfileAttributeEntity
            {
                Id = parent.Id,
                Name = parent.Name,
            };
        }

        public NdSecurityTemplate[] GetCabinetSecurityTemplates(string cabinetId)
            => SendRequest(() => v1Facade.Cabinet.GetCabinetSecurityTemplates(cabinetId));

        /// <summary>
        /// Files a document into a given filter location.
        /// </summary>
        /// <param name="filterId">Envelope id of filter.</param>
        /// <param name="documentId">Document id or envelope id.</param>
        public void FileDocumentToFilter(string filterId, string documentId)
            => SendRequest(() => v1Facade.Filter.FileDocumentToFilter(filterId, documentId));

        /// <summary>
        /// Renames a folder or a document on the server.
        /// </summary>
        /// <param name="id">Item id.</param>
        /// <param name="newName">New name of item.</param>
        /// <param name="isDocument">If true then renamed item is document, if false - folder.</param>
        /// TODO: Make this method more generic i.e. to work with workspaces, collabspaces, filters, saved searches.
        ///       We can use anyType parameter from the Rest call here.
        public NdDocumentStat RenameItem(string id, string newName, bool isDocument)
            => SendRequest(() => isDocument ? v1Facade.Document.RenameDocument(id, newName)
                                            : v1Facade.Folder.RenameFolder(id, newName));

        /// <summary>
        /// Gets the filter information.
        /// </summary>
        /// <param name="id">Filter id.</param>
        /// TODO: Make this method more generic i.e. to work with workspaces, collabspaces, filters, saved searches.
        ///       We can use anyType parameter from the Rest call here.
        public NdDocumentInformation GetFilterInfo(string id)
            => SendRequest(() => v1Facade.Filter.GetFilterInfo(id));

        /// <summary>
        /// Gets lookup values for an attribute.
        /// </summary>
        /// <param name="repositoryId">The repository id.</param>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="parentValue">The parent value if the attribute is a child attribute.</param>
        /// <param name="filter">The criteria for filtering the set of records returned.</param>
        /// <param name="top">Maximum of records to return.</param>
        public NdLookupList GetLookup(string repositoryId,
                                      int attributeId,
                                      string parentValue = null,
                                      string filter = null,
                                      int? top = null)
            => SendRequest(() => v1Facade.CustomAttributeValues.GetLookup(repositoryId,
                                                                          attributeId,
                                                                          parentValue: parentValue,
                                                                          filter: filter,
                                                                          top: top,
                                                                          orderBy: NdLookupOrderByType.Key));

        public NdContentItemsListWithAttributes GetWorkspaceDocuments(string workspaceId,
                                                                      string documentExtension,
                                                                      string skipToken,
                                                                      List<int> profileAttributeIdsToLoad)
            => SendRequest(() => v1Facade.Workspace.GetWorkspaceDocuments(workspaceId, documentExtension, skipToken, profileAttributeIdsToLoad));

        public NdDocumentStat Checkout(string documentId, bool addToRecent, int? version)
            => SendRequest(() => v1Facade.Document.Checkout(documentId, addToRecent, version));

        public NdDocumentStat CreateDocumentCopy(string filingContainer,
                                                 bool isCabinetDestination,
                                                 bool addToRecent,
                                                 string documentId,
                                                 string documentName)
            => SendRequest(() => v1Facade.Document.CreateDocumentCopy(filingContainer, isCabinetDestination, addToRecent, documentId, documentName));

        public bool RevokeAccessTokens()
            => oAuthFacade.RevokeTokens();

        private T SendRequest<T>(Func<OperationResult<T>> request,
                                 int timeoutMilliSeconds = 30000,
                                 int retryRateDelayMilliSeconds = 1000)
            where T : class
        {
            try
            {
                return Wait.ForResult(() =>
                {
                    try
                    {
                        return ActionHandler(request);
                    }
                    catch (BadRequestException ex)
                    {
                        // Note: in this case we haven't any reason to retry this action.
                        Console.WriteLine($"Access violation in REST query with error: {ex.Message}.");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"REST request failed with {ex.Message}.");
                        return null;
                    }
                },
                timeoutMilliSeconds,
                retryRateDelayMilliSeconds);
            }
            catch
            {
                return null;
            }
        }

        private bool SendRequest(Func<OperationResult> request,
                                 int timeoutMilliSeconds = 30000,
                                 int retryRateDelayMilliSeconds = 1000)
        {
            try
            {
                return Wait.ForResult(() =>
                {
                    try
                    {
                        return ActionHandler(request);
                    }
                    catch (BadRequestException ex)
                    {
                        // Note: in this case we haven't any reason to retry this action.
                        Console.WriteLine($"Access violation in REST query with error: {ex.Message}.");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"REST request failed with {ex.Message}.");
                        return false;
                    }
                },
                timeoutMilliSeconds,
                retryRateDelayMilliSeconds);
            }
            catch
            {
                return false;
            }
        }

        private bool ActionHandler(Func<OperationResult> action)
        {
            var requestResult = action?.Invoke();
            ProcessRequestResult(requestResult);

            return requestResult.IsSuccessful;
        }

        private T ActionHandler<T>(Func<OperationResult<T>> action)
            where T : class
        {
            var requestResult = action?.Invoke();
            ProcessRequestResult(requestResult);

            if (requestResult?.HasResult == true)
            {
                return requestResult.ToResultOrException();
            }

            return null;
        }

        private void ProcessRequestResult(OperationResult requestResult)
        {
            if (requestResult == null)
            {
                throw new ArgumentNullException("REST request result is null.");
            }

            if (!requestResult.IsSuccessful)
            {
                switch (requestResult.HttpStatusCode)
                {
                    case 400:
                        throw new BadRequestException(requestResult.StatusMessage);

                    case 401:
                        Console.WriteLine("Refreshing REST client.");
                        oAuthFacade.RefreshTokens();
                        break;
                }

                Console.WriteLine($"Http response code: {requestResult.HttpStatusCode}.");
                Console.WriteLine($"Request status: {requestResult.Status.ToString()}.");
                Console.WriteLine($"Request status message: {requestResult.StatusMessage}.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RevokeAccessTokens();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}
