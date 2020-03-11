using System;
using System.Runtime.InteropServices;

using Word = Microsoft.Office.Interop.Word;

using NetDocuments.Automation.Common.Exceptions.Interop;
using NetDocuments.Automation.Helpers;
using System.Collections.Generic;

namespace NetDocuments.Automation.TestDataManagement.Helpers
{
    public static class MsWordDocumentHelper
    {
        private static object interopProcessLockObject = new object();

        /// <summary>
        /// Holds Word application name for interact trough the office interop.
        /// </summary>
        public const string MSWORD_INTEROP_APP_NAME = "Word.Application";

        /// <summary>
        /// Adds passed string into an active word document.
        /// </summary>
        /// <param name="value">String to add to workbook.</param>
        /// <exception cref="ArgumentNullException">Thrown when the content string is null.</exception>
        /// <exception cref="InvalidInteropOperationException">Thrown when content cannot be added.</exception>
        public static void AddDocumentContent(string value)
        {
            using (var comHelper = new COMObjectsHelper())
            {
                var wordApp = GetActiveWordApp(comHelper)
                    ?? throw new InvalidInteropOperationException($"Could not get interop instance of {MSWORD_INTEROP_APP_NAME} with active window.");

                // Note: This property controls the visibility of alert dialogs
                wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;

                var document = comHelper.Register(() => GetDocument(wordApp))
                    ?? throw new InvalidInteropOperationException($"MS Word active document wasn't found.");

                AddDocumentContent(document, value);
            }
        }

        /// <summary>
        /// Adds passed string into an active word document.
        /// </summary>
        /// <param name="value">String to add to document.</param>
        /// <param name="title">Title in a correct format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the content string is null.</exception>
        /// <exception cref="InvalidInteropOperationException">Thrown when content cannot be added.</exception>
        public static void AddDocumentContent(string value, string title)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Content should not be null.");
            }

            using (var comHelper = new COMObjectsHelper())
            {
                var wordApp = GetActiveWordApp(comHelper)
                    ?? throw new InvalidInteropOperationException($"Could not find any {MSWORD_INTEROP_APP_NAME} application with active window.");

                // Note: This property controls the visibility of alert dialogs
                wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;

                var windows = comHelper.Register(() => wordApp.Windows);

                // Note: It may happen when we have multiple processes of app and the object given by interop doesn't have windows.
                if (windows.Count == 0)
                {
                    throw new InvalidInteropOperationException("MS Word application interop object doesn't contain windows.");
                }

                var document = FindDocument(windows, comHelper, title)
                    ?? throw new InvalidInteropOperationException($"MS Word document with a given title: {title} wasn't found.");

                AddDocumentContent(document, value);
            }
        }

        public static void CreateNewDocument(string path, string[] content)
        {
            lock (interopProcessLockObject)
            {
                using (var comHelper = new COMObjectsHelper())
                {
                    Word.Application wordApp = null;
                    try
                    {
                        var isVisible = false;

                        wordApp = comHelper.Register(() => new Word.Application());
                        wordApp.Visible = isVisible;

                        var documents = comHelper.Register(() => wordApp.Documents);

                        object readOnly = false;
                        object isAppVisible = isVisible;
                        object documentType = Word.WdNewDocumentType.wdNewBlankDocument;

                        var document = comHelper.Register(() => documents.Add(DocumentType: ref documentType, Visible: ref isAppVisible));
                        var docContent = comHelper.Register(() => document.Content);
                        docContent.Text = string.Join(Environment.NewLine, content);

                        var paragraphs = comHelper.Register(() => document.Paragraphs);
                        var lastParagraph = comHelper.Register(() => paragraphs.Last);
                        var range = comHelper.Register(() => lastParagraph.Range);
                        if (range.Text.Trim() == string.Empty)
                        {
                            range.Select();
                            var selection = comHelper.Register(() => wordApp.Selection);
                            selection.Delete();
                        }

                        object savePath = path.Clone();
                        document.SaveAs2(ref savePath);
                        document.Close();
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
                        wordApp?.Quit();
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
                    Word.Application wordApp = null;
                    try
                    {
                        var isVisible = false;

                        wordApp = comHelper.Register(() => new Word.Application());
                        wordApp.Visible = isVisible;

                        var documents = comHelper.Register(() => wordApp.Documents);

                        object readOnly = false;
                        object isAppVisible = isVisible;
                        object documentType = Word.WdNewDocumentType.wdNewBlankDocument;

                        var document = comHelper.Register(() => documents.Add(DocumentType: ref documentType, Visible: ref isAppVisible));
                        var docContent = comHelper.Register(() => document.Content);
                        docContent.Text = string.Join(" ", string.Concat(content));

                        var paragraphs = comHelper.Register(() => document.Paragraphs);
                        var lastParagraph = comHelper.Register(() => paragraphs.Last);
                        var range = comHelper.Register(() => lastParagraph.Range);
                        if (range.Text.Trim() == string.Empty)
                        {
                            range.Select();
                            var selection = comHelper.Register(() => wordApp.Selection);
                            selection.Delete();
                        }

                        object savePath = path.Clone();
                        document.SaveAs2(ref savePath);
                        document.Close();
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
                        wordApp?.Quit();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a title of an active word document.
        /// </summary>
        /// <returns>Active document title.</returns>
        public static string GetActiveDocumentTitle()
        {
            using (var comHelper = new COMObjectsHelper())
            {
                var wordApp = GetActiveWordApp(comHelper)
                    ?? throw new InvalidInteropOperationException($"Could not find any {MSWORD_INTEROP_APP_NAME} application with active window.");

                return comHelper.Register(() => wordApp.ActiveWindow)?.Caption;
            }
        }

        /// <summary>
        /// Gets full file name that is currently opened in an active document.
        /// </summary>
        /// <returns>Full file name.</returns>
        public static string GetActiveDocumentFileName()
        {
            using (var comHelper = new COMObjectsHelper())
            {
                var wordApp = GetActiveWordApp(comHelper);

                return comHelper.Register(() => wordApp.ActiveDocument)?.FullName;
            }
        }

        private static Word.Application GetActiveWordApp(COMObjectsHelper comHelper)
        {
            var wordApplications = COMObjectsHelper.GetActiveInteropApp<Word.Application>(comHelper, MSWORD_INTEROP_APP_NAME)
                ?? throw new InvalidInteropOperationException($"Could not find any {MSWORD_INTEROP_APP_NAME} application.");

            return wordApplications.Find(app => comHelper.Register(() => app?.ActiveWindow) != null);
        }

        private static Word.Document FindDocument(Word.Windows windows, COMObjectsHelper comHelper, string title)
        {
            Word.Window matchingWindow = null;

            for (var i = 1; i <= windows.Count; i++)
            {
                var window = comHelper.Register(() => windows[i]);

                if (title.Contains(window.Caption))
                {
                    matchingWindow = window;
                    break;
                }
            }

            return comHelper.Register(() => matchingWindow?.Document);
        }

        private static void AddDocumentContent(Word.Document document, string value)
        {
            using (var comHelper = new COMObjectsHelper())
            {
                try
                {
                    if (document == null)
                    {
                        throw new InvalidInteropOperationException("Could not update content for the document because document is null.");
                    }

                    var content = comHelper.Register(() => document.Content);

                    if (content == null)
                    {
                        throw new InvalidInteropOperationException($"Could not obtain current content for document {document.Name}.");
                    }

                    var isDocumentEmpty = content.End == 1;

                    if (isDocumentEmpty)
                    {
                        document.Content.Text = value;
                    }
                    else
                    {
                        var paragraphs = comHelper.Register(() => document.Paragraphs);
                        var paragraph = comHelper.Register(() => paragraphs.Add());
                        var range = comHelper.Register(() => paragraph.Range);
                        range.InsertAfter(value);
                    }
                }
                catch (COMException ex)
                {
                    throw new InvalidInteropOperationException("Failed to add content to the document, because of COMException.", ex);
                }
            }
        }

        private static Word.Document GetDocument(Word.Application wordApp)
        {
            return Wait.ForResult(() =>
            {
                try
                {
                    return wordApp.ActiveDocument;
                }
                catch (Exception ex) when (ex is COMException)
                {
                    Console.WriteLine($"Could not get Word document because of {ex.Message}.");
                    Console.WriteLine(ex);

                    return null;
                }
            },
            timeoutMilliSeconds: 60000,
            retryRateDelayMilliSeconds: 1000);
        }
    }
}
