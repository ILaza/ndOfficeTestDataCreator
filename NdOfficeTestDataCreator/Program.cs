using System;

using NetDocuments.Automation.Common.Settings;
using NetDocuments.Automation.RestClient.Factories;
using NetDocuments.Automation.TestDataManagement.Implementation;
using NetDocuments.Automation.TestDataManagement.Entities;
using NetDocuments.Automation.Helpers.Extensibility;
using NetDocuments.Automation.Helpers.Entities;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using StackExchange.Profiling;

namespace NdOfficeTestDataCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            const int  SIZE_FILE = 500;

            var hostDucot = new HostSettings("api.ducot.netdocuments.com", "ducot.netdocuments.com", "https://api.ducot.netdocuments.com",
                                             "AP-BD15GCDS", "n9xbjBqraJ9iBuJIY8eQy59a9rRI7tm26oluLUkkNMMxzSBe", "https://localhost/");

            var apiFactory = new DefaultRestApiFactory(hostDucot);

            var repo = new NdRestRepository(apiFactory.CreateApiClient(new NetDocuments.Automation.Common.Entities.UserInfo("nonAdminPerformanceUser", "r3ward$777")), 
                                            PerformanceCredentials.repositoryID,
                                            PerformanceCredentials.cabinetFirstID, 
                                            new NdFoldersEntity());

            var documentCreator = new DocumentCreator(repo, SIZE_FILE);

            WebDocumentInfo newDocumentInfo;

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 1; i++)
            {
                newDocumentInfo = documentCreator.GetNewDocument("docx", SIZE_FILE);
                DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            }

            for (int i = 0; i < 1; i++)
            {
                newDocumentInfo = documentCreator.GetNewDocument("xlsx", SIZE_FILE);
                DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            }

            for (int i = 0; i < 1; i++)
            {
                newDocumentInfo = documentCreator.GetNewDocument("pptx", SIZE_FILE);
                DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            }

            //Parallel.ForEach(Enumerable.Range(0, 9), (i) =>
            //{
            //    newDocumentInfo = documentCreator.GetNewDocument("pptx", SIZE_FILE);
            //    DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            //});


            //var createdDocs = Enumerable.Range(1, 100)
            //          .AsParallel()
            //          .Select(_ =>
            //          {
            //              try
            //              {
            //                  var newDocumentInfo = documentCreator.GetNewDocument("pptx", SIZE_FILE);
            //                  DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            //                  return true;
            //              }
            //              catch (Exception ex)
            //              {
            //                  Console.WriteLine(ex.Message);
            //                  return false;
            //              }
            //          })
            //          .ToList();
            Console.WriteLine(sw.Elapsed);
                    //documentCreator.ClearAlldata();
        }
    }
}