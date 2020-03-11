using System;
using System.Runtime.InteropServices;

using Excel = Microsoft.Office.Interop.Excel;

using NetDocuments.Automation.Common.Exceptions.Interop;
using NetDocuments.Automation.Helpers;
using System.Collections.Generic;

namespace NetDocuments.Automation.TestDataManagement.Helpers
{
    /// <summary>
    /// Holds methods to work with excel documents.
    /// </summary>
    public static class MsExcelDocumentHelper
    {
        private static object interopProcessLockObject = new object();

        /// <summary>
        /// Holds Excel application name for interact trough the office interop.
        /// </summary>
        public const string MSEXCEL_INTEROP_APP_NAME = "Excel.Application";

        public static void CreateNewDocument(string path, string[] content)
        {
            lock (interopProcessLockObject)
            {
                using (var comHelper = new COMObjectsHelper())
                {
                    Excel.Application excelApp = null;
                    try
                    {
                        excelApp = comHelper.Register(() => new Excel.Application());

                        var books = comHelper.Register(() => excelApp.Workbooks);

                        object readOnly = false;

                        Excel.Workbook book = comHelper.Register(() => books.Add());

                        var sheet = comHelper.Register(() => book.ActiveSheet);

                        AddDocumentContent(sheet, content);

                        object savePath = path.Clone();
                        book.SaveAs(savePath);
                        book.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Document creation failed with error: {ex.Message}. Exception: {ex}.");
                        throw;
                    }
                    finally
                    {
                        // Note: We always should manually quit from application,
                        // because Release() only decreases references counter.
                        excelApp?.Quit();
                    }
                }
            }
        }

        public static void CreateNewDocumentPerformance(string path, IEnumerable<char> content)
        {
            lock (interopProcessLockObject)
            {
                using (var comHelper = new COMObjectsHelper())
                {
                    Excel.Application excelApp = null;
                    try
                    {
                        excelApp = comHelper.Register(() => new Excel.Application());

                        var books = comHelper.Register(() => excelApp.Workbooks);

                        object readOnly = false;

                        Excel.Workbook book = comHelper.Register(() => books.Add());

                        var sheet = comHelper.Register(() => book.ActiveSheet);

                        AddDocumentContentPerformance(sheet, content);

                        object savePath = path.Clone();
                        book.SaveAs(savePath);
                        book.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Document creation failed with error: {ex.Message}. Exception: {ex}.");
                        throw;
                    }
                    finally
                    {
                        // Note: We always should manually quit from application,
                        // because Release() only decreases references counter.
                        excelApp?.Quit();
                    }
                }
            }
        }

        /// <summary>
        /// Adds passed string into active excel workbook in the first free cell.
        /// </summary>
        /// <param name="value">String to add to workbook.</param>
        /// <exception cref="ArgumentNullException">Thrown when the content string is null.</exception>
        /// <exception cref="InvalidInteropOperationException">Thrown when content cannot be added.</exception>
        public static void AddDocumentContent(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Content should not be null.");
            }

            // Note: When add content happens in the same time with create new document it breaks excel tests.
            lock (interopProcessLockObject)
            {
                using (var comHelper = new COMObjectsHelper())
                {
                    var application = GetActiveExcelApp(comHelper)
                        ?? throw new InvalidInteropOperationException("Could not get active excel application object.");

                    // Note: This property controls the visibility of alert dialogs
                    application.DisplayAlerts = false;

                    var activeSheet = comHelper.Register(() => (Excel.Worksheet)application.ActiveSheet);

                    AddDocumentContent(activeSheet, value);
                }
            }
        }

        /// <summary>
        /// Adds passed string into an active excel workbook in the first free cell.
        /// </summary>
        /// <param name="value">String to add to workbook.</param>
        /// <param name="title">Title in a correct format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the content string is null.</exception>
        /// <exception cref="InvalidInteropOperationException">Thrown when content cannot be added.</exception>
        public static void AddDocumentContent(string value, string title)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Content should not be null.");
            }

            // Note: When add content happens in the same time with create new document it breaks excel tests.
            lock (interopProcessLockObject)
            {
                using (var comHelper = new COMObjectsHelper())
                {
                    var excelApp = GetActiveExcelApp(comHelper)
                        ?? throw new InvalidInteropOperationException("Cannot obtain MS Excel application interop object from ROT table.");

                    // Note: This property controls the visibility of alert dialogs
                    excelApp.DisplayAlerts = false;

                    var windows = comHelper.Register(() => excelApp.Windows);

                    // Note: It may happen when we have multiple processes of app and the object given by interop doesn't have windows.
                    if (windows.Count == 0)
                    {
                        throw new InvalidInteropOperationException("MS Excel application interop object doesn't contain windows.");
                    }

                    var activeSheet = FindActiveSheet(windows, comHelper, title)
                        ?? throw new InvalidInteropOperationException($"MS Excel sheet with a given title: {title} wasn't found.");

                    AddDocumentContent(activeSheet, value);
                }
            }
        }

        /// <summary>
        /// Gets title of an active excel workbook.
        /// </summary>
        /// <returns>Active workbook title.</returns>
        public static string GetActiveWorkbookTitle()
        {
            using (var comHelper = new COMObjectsHelper())
            {
                var excelApp = GetActiveExcelApp(comHelper)
                    ?? throw new InvalidInteropOperationException("Cannot obtain MS Excel application interop object from ROT table.");

                return comHelper.Register(() => excelApp.ActiveWindow)?.Caption;
            }
        }

        /// <summary>
        /// Gets full file name that is currently opened in an active workbook.
        /// </summary>
        /// <returns>Full file name.</returns>
        public static string GetActiveWorkbookFileName()
        {
            using (var comHelper = new COMObjectsHelper())
            {
                var excelApp = GetActiveExcelApp(comHelper)
                    ?? throw new InvalidInteropOperationException("Cannot obtain MS Excel application interop object from ROT table.");

                return comHelper.Register(() => excelApp.ActiveWorkbook)?.FullName;
            }
        }

        private static Excel.Worksheet FindActiveSheet(Excel.Windows windows, COMObjectsHelper comHelper, string title)
        {
            Excel.Window matchingWindow = null;

            for (var i = 1; i <= windows.Count; i++)
            {
                var window = comHelper.Register(() => windows[i]);

                if (title.Contains(window.Caption))
                {
                    matchingWindow = window;
                    break;
                }
            }

            return comHelper.Register(() => matchingWindow?.ActiveSheet);
        }

        /// <summary>
        /// Adds passed string into an active excel workbook in the first free cell.
        /// </summary>
        /// <param name="sheet">Excel worksheet to add a content.</param>
        /// <param name="values">Strings to add to workbook.</param>
        private static void AddDocumentContent(Excel.Worksheet sheet, params string[] values)
        {
            using (var comHelper = new COMObjectsHelper())
            {
                try
                {
                    Excel.Range activeCell = null;
                    var rowIndex = 0;
                    var cells = comHelper.Register(() => sheet.Cells);
                    do
                    {
                        ++rowIndex;
                        activeCell = comHelper.Register(() => cells[rowIndex, 1]);
                    } while (!string.IsNullOrEmpty((string)activeCell.Value2));

                    foreach (var value in values)
                    {
                        activeCell.Value2 = value;
                        ++rowIndex;
                        activeCell = comHelper.Register(() => cells[rowIndex, 1]);
                    }
                }
                catch (COMException)
                {
                    throw new InvalidInteropOperationException("Failed to add content to the worksheet, because of COMException.");
                }
            }
        }

        private static void AddDocumentContentPerformance(Excel.Worksheet sheet, IEnumerable<char> values)
        {
            using (var comHelper = new COMObjectsHelper())
            {
                try
                {
                    Excel.Range activeCell = null;
                    var rowIndex = 0;
                    var cells = comHelper.Register(() => sheet.Cells);
                    do
                    {
                        ++rowIndex;
                        activeCell = comHelper.Register(() => cells[rowIndex, 1]);
                    } while (!string.IsNullOrEmpty((string)activeCell.Value2));

                    foreach (var value in values)
                    {
                        activeCell.Value2 = value;
                        ++rowIndex;
                        activeCell = comHelper.Register(() => cells[rowIndex, 1]);
                    }
                }
                catch (COMException)
                {
                    throw new InvalidInteropOperationException("Failed to add content to the worksheet, because of COMException.");
                }
            }
        }

        private static Excel.Application GetActiveExcelApp(COMObjectsHelper comHelper)
        {
            var excelApplications = COMObjectsHelper.GetActiveInteropApp<Excel.Application>(comHelper, MSEXCEL_INTEROP_APP_NAME)
                ?? throw new InvalidInteropOperationException($"Could not find any {MSEXCEL_INTEROP_APP_NAME} application.");

            return excelApplications.Find(app => comHelper.Register(() => app.ActiveWindow) != null);
        }
    }
}
