using System;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

using NetDocuments.Automation.Common.Exceptions.Interop;
using NetDocuments.Automation.Helpers;
using System.Collections.Generic;

namespace NetDocuments.Automation.TestDataManagement.Helpers
{
    /// <summary>
    /// Holds methods to work with PowerPoint documents.
    /// </summary>
    public static class MsPowerPointDocumentHelper
    {
        private static object interopProcessLockObject = new object();

        /// <summary>
        /// Holds PowerPoint application name for interact trough the office interop.
        /// </summary>
        public const string MSPOWERPOINT_INTEROP_APP_NAME = "PowerPoint.Application";

        /// <summary>
        /// Adds passed string into presentation in the empty shape or in the newly created one.
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

            using (var comHelper = new COMObjectsHelper())
            {
                var powerpointApp = GetActivePowerPointApp(comHelper)
                    ?? throw new InvalidInteropOperationException($"Could not find any {MSPOWERPOINT_INTEROP_APP_NAME} application with active window.");

                // Note: This property controls the visibility of alert dialogs
                powerpointApp.DisplayAlerts = PowerPoint.PpAlertLevel.ppAlertsNone;

                var presentation = comHelper.Register(() => powerpointApp.ActivePresentation);

                AddDocumnetContent(presentation, value);
            }
        }

        /// <summary>
        /// Adds passed string into presentation in the empty shape or in the newly created one.
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

            using (var comHelper = new COMObjectsHelper())
            {
                var powerpointApp = GetActivePowerPointApp(comHelper)
                    ?? throw new InvalidInteropOperationException($"Could not find any {MSPOWERPOINT_INTEROP_APP_NAME} application with active window.");

                // Note: This property controls the visibility of alert dialogs
                powerpointApp.DisplayAlerts = PowerPoint.PpAlertLevel.ppAlertsNone;

                var windows = comHelper.Register(() => powerpointApp.Windows);

                // Note: It may happen when we have multiple processes of app and the object given by interop doesn't have windows.
                if (windows.Count == 0)
                {
                    throw new InvalidInteropOperationException("MS PowerPoint application interop object doesn't contain windows.");
                }

                var presentation = FindPresentation(windows, comHelper, title)
                    ?? throw new InvalidInteropOperationException($"MS PowerPoint presentation with a given title: {title} wasn't found.");

                AddDocumnetContent(presentation, value);
            }
        }

        /// <summary>
        /// Clears all content in document.
        /// </summary>
        public static void ClearDocumentContent()
        {
            using (var comHelper = new COMObjectsHelper())
            {
                var powerpointApp = GetActivePowerPointApp(comHelper)
                    ?? throw new InvalidInteropOperationException($"Could not find any {MSPOWERPOINT_INTEROP_APP_NAME} application with active window.");

                var presentation = comHelper.Register(() => powerpointApp.ActivePresentation);
                var slides = comHelper.Register(() => presentation.Slides);

                RemoveSlides(slides, comHelper, forceRemoveAll: true);
                CreateNewSlideWithEmptyShape(presentation, comHelper);
            }
        }

        /// <summary>
        /// Gets title of an active powerpoint presentation.
        /// </summary>
        /// <returns>Active workbook title.</returns>
        public static string GetActiveDocumentTitle()
        {
            using (var comHelper = new COMObjectsHelper())
            {
                var powerpointApp = GetActivePowerPointApp(comHelper)
                    ?? throw new InvalidInteropOperationException($"Could not find any {MSPOWERPOINT_INTEROP_APP_NAME} application with active window.");

                return comHelper.Register(() => powerpointApp.ActiveWindow)?.Caption;
            }
        }

        /// <summary>
        /// Gets full file name that is currently opened in an active powerpoint presentation.
        /// </summary>
        /// <returns>Full file name.</returns>
        public static string GetActiveDocumentFileName()
        {
            using (var comHelper = new COMObjectsHelper())
            {
                var powerpointApp = GetActivePowerPointApp(comHelper)
                    ?? throw new InvalidInteropOperationException($"Could not find any {MSPOWERPOINT_INTEROP_APP_NAME} application with active window.");

                return comHelper.Register(() => powerpointApp.ActivePresentation)?.FullName;
            }
        }

        public static void CreateNewDocument(string path, IEnumerable<char> content)
        {
            lock (interopProcessLockObject)
            {
                using (var comHelper = new COMObjectsHelper())
                {
                    PowerPoint.Application ppApp = null;
                    try
                    {
                        Wait.ForResult(() =>
                        {
                            try
                            {
                                ppApp = comHelper.Register(() => new PowerPoint.Application());
                                CreateNewDocument(comHelper, ppApp, path, content);
                            }
                            catch (COMException ex)
                            {
                                Console.WriteLine($"Document creation failed with error: {ex.Message}. Exception: {ex}.");
                            }

                            return File.Exists(path);
                        });
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
                        ppApp?.Quit();
                    }
                }
            }
        }

        private static void CreateNewDocument(COMObjectsHelper comHelper, PowerPoint.Application ppApp, string path, IEnumerable<char> content)
        {
            var presentations = comHelper.Register(() => ppApp.Presentations);

            object readOnly = false;

            var presentation = comHelper.Register(() => presentations.Add(MsoTriState.msoFalse));

            AddDocumentContent(presentation, string.Concat(content));

            presentation.SaveAs(path);
            presentation.Close();
        }

        private static void AddDocumentContent(PowerPoint.Presentation presentation, string content)
        {
            using (var comHelper = new COMObjectsHelper())
            {
                var slides = comHelper.Register(() => presentation.Slides);
                PrepareDocument(presentation, slides, comHelper);

                var targetShape = FindEmptyShape(slides, comHelper) ?? CreateNewSlideWithEmptyShape(presentation, comHelper);

                var targetFrame = comHelper.Register(() => targetShape.TextFrame);
                var textRange = comHelper.Register(() => targetFrame.TextRange);
                textRange.Text = content;
            }
        }

        private static void PrepareDocument(PowerPoint.Presentation presentation, PowerPoint.Slides slides, COMObjectsHelper comHelper)
        {
            RemoveShapesWithEmptyTextFrame(slides, comHelper);
            RemoveSlides(slides, comHelper);

            if (slides.Count == 0)
            {
                CreateNewSlideWithEmptyShape(presentation, comHelper);
            }
        }

        private static void RemoveSlides(PowerPoint.Slides slides, COMObjectsHelper comHelper, bool forceRemoveAll = false)
        {
            for (int i = slides.Count; i > 0; --i)
            {
                var slide = comHelper.Register(() => slides[i]);
                if (forceRemoveAll || IsSlideEmpty(slide, comHelper))
                {
                    slide.Delete();
                }
            }
        }

        private static bool IsSlideEmpty(PowerPoint.Slide slide, COMObjectsHelper comHelper)
            => comHelper.Register(() => slide.Shapes).Count == 0;

        private static void RemoveShapesWithEmptyTextFrame(PowerPoint.Slides slides, COMObjectsHelper comHelper)
        {
            foreach (PowerPoint.Slide slide in slides)
            {
                comHelper.Register(() => slide);
                var shapes = comHelper.Register(() => slide.Shapes);
                for (int i = shapes.Count; i > 0; --i)
                {
                    var shape = comHelper.Register(() => shapes[i]);
                    if (shape.HasTextFrame == MsoTriState.msoTrue
                        && comHelper.Register(() => shape.TextFrame2).HasText == MsoTriState.msoFalse)
                    {
                        shape.Delete();
                    }
                }
            }
        }

        private static PowerPoint.Shape FindEmptyShape(PowerPoint.Slides slides, COMObjectsHelper comHelper)
        {
            foreach (PowerPoint.Slide slide in slides)
            {
                comHelper.Register(() => slide);
                foreach (PowerPoint.Shape shape in slide.Shapes)
                {
                    comHelper.Register(() => shape);
                    if (shape.HasTextFrame == MsoTriState.msoTrue
                        && comHelper.Register(() => shape.TextFrame).HasText == MsoTriState.msoFalse)
                    {
                        return shape;
                    }
                }
            }

            return null;
        }

        private static PowerPoint.Shape CreateNewSlideWithEmptyShape(PowerPoint.Presentation presentation, COMObjectsHelper comHelper)
        {
            var slides = comHelper.Register(() => presentation.Slides);
            var slideMaster = comHelper.Register(() => presentation.SlideMaster);
            var layouts = comHelper.Register(() => slideMaster.CustomLayouts);
            var customLayout = layouts[PowerPoint.PpSlideLayout.ppLayoutChartAndText];

            var newSlide = comHelper.Register(() => slides.AddSlide(slides.Count + 1, customLayout));

            return comHelper.Register(() => newSlide.Shapes[1]);
        }

        private static PowerPoint.Application GetActivePowerPointApp(COMObjectsHelper comHelper)
        {
            var ppApplications = COMObjectsHelper.GetActiveInteropApp<PowerPoint.Application>(comHelper, MSPOWERPOINT_INTEROP_APP_NAME)
                ?? throw new InvalidInteropOperationException($"Could not find any {MSPOWERPOINT_INTEROP_APP_NAME} application.");

            return ppApplications.Find(app => comHelper.Register(() => app.ActiveWindow) != null);
        }

        private static void AddDocumnetContent(PowerPoint.Presentation presentation, string value)
        {
            using (var comHelper = new COMObjectsHelper())
            {
                try
                {
                    var slides = comHelper.Register(() => presentation.Slides);
                    PrepareDocument(presentation, slides, comHelper);

                    var targetShape = FindEmptyShape(slides, comHelper) ?? CreateNewSlideWithEmptyShape(presentation, comHelper);

                    var targetFrame = comHelper.Register(() => targetShape.TextFrame);
                    var textRange = comHelper.Register(() => targetFrame.TextRange);
                    textRange.Text = value;
                }
                catch (COMException)
                {
                    throw new InvalidInteropOperationException("Failed to add content to the presentation, because of COMException.");
                }
            }
        }

        private static PowerPoint.Presentation FindPresentation(PowerPoint.DocumentWindows windows,
                                                                COMObjectsHelper comHelper,
                                                                string title)
        {
            PowerPoint.DocumentWindow matchingWindow = null;

            for (var i = 1; i <= windows.Count; i++)
            {
                var window = comHelper.Register(() => windows[i]);

                if (title.Contains(window.Caption))
                {
                    matchingWindow = window;
                    break;
                }
            }

            return comHelper.Register(() => matchingWindow?.Presentation);
        }
    }
}
