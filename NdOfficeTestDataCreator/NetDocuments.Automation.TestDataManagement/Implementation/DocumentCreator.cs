using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetDocuments.Rest.Contracts.Models.V1;

using NetDocuments.Automation.Helpers;
using NetDocuments.Automation.Helpers.Entities;
using NetDocuments.Automation.TestDataManagement.Abstract;
using NetDocuments.Automation.TestDataManagement.Helpers;

namespace NetDocuments.Automation.TestDataManagement.Implementation
{
    /// <summary>
    /// Class for create MS Office application documents.
    /// </summary>
    public sealed class DocumentCreator : IDisposable
    {
        private INdWebRepository repository;

        private static FileSystemHelper fsHelper = new FileSystemHelper();

        private Dictionary<string, ConcurrentQueue<WebDocumentInfo>> availableDocs = new Dictionary<string, ConcurrentQueue<WebDocumentInfo>>();
        private Queue<string> persistedDocs = new Queue<string>();

        private readonly int availDocsInQueue;
        private List<string> documentsInUse = new List<string>();
        private List<string> documentsToDelete = new List<string>();

        private delegate void OnDocumentActionDelegate(string docId);
        private delegate void OnDocumentTakenDelegate(string docType, string docId);
        private event OnDocumentTakenDelegate DocumentTaken;
        private event OnDocumentActionDelegate DocumentReleased;

        private Dictionary<string, Func<string, IEnumerable<char>, string>> documentCreators;
        public IFoldersEntity FoldersStructure => repository.FoldersStructure;

        /// <summary>
        /// Creates new instance of the DocumentCreator class.
        /// </summary>
        /// <param name="repository">Documents repository object.</param>
        /// <param name="availDocsInQueue">Number of each type documents in the queue.</param>
        /// TODO: update to diff count of documents in queue
        public DocumentCreator(INdWebRepository repository, int sizeFile, int availDocsInQueue = 0, bool shouldClearData = true)
        {
            this.repository = repository;
            this.availDocsInQueue = availDocsInQueue;

            documentCreators = new Dictionary<string, Func<string, IEnumerable<char>, string>>
            {
                ["doc"]  = CreateWordDocument,
                ["docx"] = CreateWordDocument,
                ["xlsx"] = CreateExcelDocument,
                ["pptx"] = CreatePowerPointDocument,
            };

            DocumentTaken += DocumentCreator_DocumentTaken;
            DocumentReleased += DocumentCreator_DocumentReleased;

            Parallel.ForEach(documentCreators.Keys, (key) =>
            {
                var queue = new ConcurrentQueue<WebDocumentInfo>();
                CreateDocsInQueue(queue, key, docsCount: availDocsInQueue, sizeFile).Wait();
                availableDocs[key] = queue;
            });
        }

        /// <summary>
        /// Gets document with type from queue.
        /// </summary>
        /// <param name="docType">Document type, e.g.: docx, pptx, xlsx.</param>
        /// <returns>Document information.</returns>
        /// <exception cref="ArgumentException">When document type is wrong.</exception>
        public WebDocumentInfo GetDocument(string docType, int sizeFile)
        {
            if (docType == null || !availableDocs.TryGetValue(docType, out var docsQueue))
            {
                throw new ArgumentException($"Could not get documents collection for type {docType}.");
            }

            if (docsQueue?.Count == 0)
            {
                return GetNewDocument(docType, sizeFile);
            }

            WebDocumentInfo doc = null;
            Wait.ForResult(() => docsQueue.TryDequeue(out doc));

            Volatile.Read(ref DocumentTaken)?.Invoke(docType, (string)doc?.Id?.Clone());
            return doc;
        }

        /// <summary>
        /// Creates new document in repository.
        /// </summary>
        /// <param name="docType">Document type.</param>
        /// <param name="folderName">Folder name to file a document.</param>
        /// <param name="profileAttributes">Collection of PAs for the new document.</param>
        /// <returns>Document information.</returns>
        /// <exception cref="ArgumentException">When document type is wrong.</exception>

        public WebDocumentInfo GetNewDocument(string docType, int sizeFile, string folderName = null, NdDocumentAttribute[] profileAttributes = null)
        {
            if (docType == null)
            {
                throw new ArgumentException("Document version type should not be null.");
            }

            if (!documentCreators.ContainsKey(docType))
            {
                throw new ArgumentException($"Version could not be created for extension: '{docType}'.");
            }

            var document = CreateNewDocument(docType, sizeFile, folderName, profileAttributes);
            documentsInUse.Add(document.Id);
            return document;
        }

        /// <summary>
        /// Creates new document with multiple versions in repository.
        /// </summary>
        /// <param name="versionsTypes">Versions types in a document.</param>
        /// <param name="folderName">Folder name to file a document.</param>
        /// <param name="contentStringsCount">Number of strings in document content.</param>
        /// <param name="includeDescriptions">Flag to decide if newly created versions should have descriptions.</param>
        /// <param name="profileAttributes">Collection of PAs for the new document.</param>
        /// <returns>Document information.</returns>
        /// <exception cref="ArgumentException">
        /// Throws when either document types collection is null or it contains incorrect document type.
        /// </exception>
        //public WebDocumentInfo GetNewDocumentWithVersions(string[] versionsTypes,
        //                                                  int sizeFile,
        //                                                  string folderName = null,
        //                                                  int contentStringsCount = 2,
        //                                                  bool includeDescriptions = false,
        //                                                  NdDocumentAttribute[] profileAttributes = null)
        //{
        //    if (versionsTypes == null)
        //    {
        //        throw new ArgumentException("Collection of version types should not be null.");
        //    }

        //    if (versionsTypes.Any(v => v == null))
        //    {
        //        throw new ArgumentException("Collection of version types should not contain version types with null value.");
        //    }

        //    if (versionsTypes.Any(v => !documentCreators.ContainsKey(v)))
        //    {
        //        var nonExistentCreators = versionsTypes.Except(documentCreators.Keys);
        //        throw new ArgumentException("One or more version could not be created for extensions: " +
        //                                    string.Join(", ", nonExistentCreators));
        //    }

        //    try
        //    {
        //        var document = CreateNewDocument(versionsTypes[0], sizeFile, folderName, profileAttributes);

        //        var unOfficialVersions = versionsTypes.Skip(1);
        //        Parallel.ForEach(unOfficialVersions,
        //                         versionType =>
        //                         {
        //                             var internalContent = RandomHelper.GetRandomStrings(contentStringsCount);
        //                             var sourceFileName = documentCreators[versionType](versionType, internalContent);

        //                             var versionDescription = includeDescriptions ? RandomHelper.GetRandomString() : string.Empty;
        //                             var newVersion = repository.CreateVersion(document.Id,
        //                                                                       versionType,
        //                                                                       sourceVersion: 1,
        //                                                                       sourceFileName,
        //                                                                       isOfficial: false,
        //                                                                       description: versionDescription);

        //                             newVersion.AddContent(internalContent);
        //                             document.Versions.Add(newVersion);
        //                         });

        //        documentsInUse.Add(document.Id);

        //        return document;
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        throw;
        //    }
        //}

        /// <summary>
        /// Prepares document for deletion.
        /// </summary>
        /// <param name="docId">Document id.</param>
        public void FreeDocument(string docId)
        {
            if (availableDocs.Values.Any(queue => queue.Any(d => d.Id.Equals(docId)))
                || documentsInUse.Any(d => !d.Equals(docId)))
            {
                throw new InvalidOperationException($"Document {docId} was not taken for test");
            }

            Volatile.Read(ref DocumentReleased)?.Invoke(docId);
        }

        /// <summary>
        /// Deletes all created data in repository.
        /// </summary>
        public void ClearAlldata()
        {
            //Wait.ForResult(() => Parallel.ForEach(documentsToDelete.Distinct(),
            //                                      repository.DeleteDocument)
            //                             .IsCompleted);

            repository.Dispose();
        }

        /// <summary>
        /// Releases all local resources.
        /// </summary>
        public void Dispose()
        {
            Parallel.ForEach(persistedDocs, path => fsHelper.DeleteFileAsync(path));

            DocumentTaken -= DocumentCreator_DocumentTaken;
            DocumentReleased -= DocumentCreator_DocumentReleased;

            var availDocs = availableDocs.Values.SelectMany(d => d.ToList()).Select(d => d.Id).ToList();

            if (availDocs.Any())
            {
                documentsToDelete.AddRange(availDocs);
            }

            if (documentsInUse.Any())
            {
                documentsToDelete.AddRange(documentsInUse);
            }

            //ClearAlldata();
        }

        private string CreateWordDocument(string docType, IEnumerable<char> content)
        {
            var path = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.{docType}");
            persistedDocs.Enqueue(path);
            MsWordDocumentHelper.CreateNewDocumentPerformance(path, content);
            return path;
        }

       
        private string CreateExcelDocument(string docType, IEnumerable<char> content)
        {
            var path = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.{docType}");
            persistedDocs.Enqueue(path);
            MsExcelDocumentHelper.CreateNewDocumentPerformance(path, content);
            return path;
        }

        private string CreatePowerPointDocument(string docType, IEnumerable<char> content)
        {
            var path = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.{docType}");
            persistedDocs.Enqueue(path);
            MsPowerPointDocumentHelper.CreateNewDocument(path, content);
            return path;
        }

        private void DocumentCreator_DocumentReleased(string docId)
        {
            documentsToDelete.Add(docId);
            documentsInUse.Remove(docId);
            repository.DeleteDocument(docId);
        }

        private void DocumentCreator_DocumentTaken(string docType, string docId)
        {
            if (docType is null || docId is null)
            {
                throw new ArgumentNullException($"Document type or document id argument is null");
            }

            documentsInUse.Add(docId);
            var queue = availableDocs[docType];

            // Note: Actually it causes a conflict with MS Office application started by interop
            // and with another one MS Office process, started by test runner.
            // So we should think up another approach for adding new documents to the queue.
            //CreateDocsInQueue(queue, docType, availDocsInQueue - queue.Count);
        }

        private WebDocumentInfo CreateNewDocument(string docType, int sizeFile, string folderName = null, NdDocumentAttribute[] profileAttributes = null)
        {
            Console.WriteLine($"Generating content with {sizeFile} length");
            var internalContent = RandomHelper.GetRandomContentPerformance(sizeFile);
            Console.WriteLine("Content generated");

            documentCreators = new Dictionary<string, Func<string, IEnumerable<char>, string>>()
            {
                ["doc"]  = CreateWordDocument,
                ["docx"] = CreateWordDocument,
                ["xlsx"] = CreateExcelDocument,
                ["pptx"] = CreatePowerPointDocument,
            };

            Console.WriteLine("Creating file..");
            var sourceFileName = documentCreators[docType](docType, internalContent);
            Console.WriteLine($"File {sourceFileName} created");

            try
            {
                var document = repository.CreateDocument(folderName,
                                                         docType,
                                                         sourceFileName,
                                                         profileAttributes);

                document.OfficialVersion.AddContentPerformance(internalContent);
                return document;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private Task CreateDocsInQueue(ConcurrentQueue<WebDocumentInfo> queue, string docType, int docsCount, int sizeFile)
        {
            if (docsCount > 0)
            {
                return Task.Run(() => Enumerable.Repeat(1, docsCount)
                                                .Select(_ => CreateNewDocument(docType, sizeFile))
                                                .ToList())
                           .ContinueWith(result => result.Result.ForEach(queue.Enqueue));
            }

            return Task.FromResult(false);
        }
    }
}
