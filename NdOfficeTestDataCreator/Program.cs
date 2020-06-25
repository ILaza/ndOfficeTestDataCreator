using System;

using NetDocuments.Automation.Common.Settings;
using NetDocuments.Automation.RestClient.Factories;
using NetDocuments.Automation.TestDataManagement.Implementation;
using NetDocuments.Automation.TestDataManagement.Entities;
using NetDocuments.Automation.Helpers.Extensibility;
using NetDocuments.Automation.Helpers.Entities;
using NetDocuments.Automation.Helpers;
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
            int  sizeFileKB = 1;
            const int WORD_COEFFICIENT = 1300;
            const int EXCEL_COEFFICIENT = 120;
            const int PP_COEFFICIENT = 200;

            const string FOLDER_WITN_SUBFOLDERS_NAME = "TestFolderWithSubfolders";
            const string FOLDER_WITN_SUBFOLDERS_ID = "4839-7613-0491";
            const int AMOUNT_OF_SUBFOLDERS = 450;

            var hostDucot = new HostSettings("api.ducot.netdocuments.com", "ducot.netdocuments.com", "https://api.ducot.netdocuments.com",
                                             "AP-BD15GCDS", "n9xbjBqraJ9iBuJIY8eQy59a9rRI7tm26oluLUkkNMMxzSBe", "https://localhost/");

            var apiFactory = new DefaultRestApiFactory(hostDucot);

            var repo = new NdRestRepository(apiFactory.CreateApiClient(new NetDocuments.Automation.Common.Entities.UserInfo("nonAdminPerformanceUser", "r3ward$777")), 
                                            PerformanceCredentials.repositoryID,
                                            PerformanceCredentials.cabinetFirstID, 
                                            new NdFoldersEntity());

            //repo.CreateSubfolders(FOLDER_WITN_SUBFOLDERS_ID, AMOUNT_OF_SUBFOLDERS);

            //var documentCreator = new DocumentCreator(repo, sizeFileKB);

            //WebDocumentInfo newDocumentInfo;

            //string pass = "C:\\Users\\ilaza\\PerformanceContent\\content_0_5_GB";
            //CreatorTxtFileHelper.CreateFile(pass, 536870912);

            //// NOTE: Create 34 ".doc" files 300 Kb average.
            //for (int i = 0; i < 1; i++)
            //{
            //    sizeFileKB = RandomHelper.GetRandomInt(110, 480);
            //    newDocumentInfo = documentCreator.GetNewDocument("doc", sizeFileKB * WORD_COEFFICIENT);
            //    DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            //}

            ////NOTE: Create ".docx" > 3MB && < 5MB
            //for (int i = 0; i < 5; i++)
            //{
            //    sizeFileKB = RandomHelper.GetRandomInt(3000, 5000);
            //    newDocumentInfo = documentCreator.GetNewDocument("doc", sizeFileKB * WORD_COEFFICIENT);
            //    DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            //}

            ////create 34 ".docx" files 1.1MB - 4.8MB
            //for (int i = 0; i < 34; i++)
            //{
            //    sizeFileKB = RandomHelper.GetRandomInt(110, 480);
            //    newDocumentInfo = documentCreator.GetNewDocument("docx", sizeFileKB * WORD_COEFFICIENT);
            //    DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            //}

            ////create 34 ".xlsx" files 1.1MB - 4.8MB
            //for (int i = 0; i < 10; i++)
            //{
            //    sizeFileKB = RandomHelper.GetRandomInt(110, 480);
            //    newDocumentInfo = documentCreator.GetNewDocument("xlsx", sizeFileKB * EXCEL_COEFFICIENT);
            //    DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            //}

            //for (int i = 0; i < 5; i++)
            //{
            //    sizeFileKB = RandomHelper.GetRandomInt(110, 400);
            //    newDocumentInfo = documentCreator.GetNewDocument("pptx", sizeFileKB * PP_COEFFICIENT);
            //    DocumentManager.AddDocumentToACWithExtensibility(newDocumentInfo.Id, newDocumentInfo.OfficialVersion.Number, true, hostDucot);
            //}

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

            //documentCreator.ClearAlldata();
        }
    }
}