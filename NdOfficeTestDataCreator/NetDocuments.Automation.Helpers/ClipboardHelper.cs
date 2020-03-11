using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Holds method to work with Clipboard.
    /// </summary>
    public static class ClipboardHelper
    {
        /// <summary>
        /// Gets text from Clipboard.
        /// </summary>
        public static string GetTextFromClipboard()
        {
            var textFromClipboard = string.Empty;

            try
            {
                var thread = new Thread(() => textFromClipboard = Clipboard.GetText());
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            catch (Exception ex) when (ex is ExternalException || ex is OutOfMemoryException)
            {
                throw new InvalidOperationException("Cannot get text from clipboard", ex);
            }

            return textFromClipboard;
        }

        /// <summary>
        /// Sets text to Clipboard.
        /// </summary>
        /// <param name="text">Text for putting into clipboard.</param>
        public static void SetTextToClipboard(string text)
        {
            try
            {
                var thread = new Thread(() => Clipboard.SetText(text));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            catch (Exception ex) when (ex is ExternalException || ex is OutOfMemoryException)
            {
                throw new InvalidOperationException("Cannot set text to clipboard", ex);
            }
        }

        /// <summary>
        /// Clears Clipboard.
        /// </summary>
        public static void ClearClipboard()
        {
            try
            {
                var thread = new Thread(() => Clipboard.Clear());
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            catch (Exception ex) when (ex is ExternalException || ex is OutOfMemoryException)
            {
                throw new InvalidOperationException("Cannot clear the clipboard", ex);
            }
        }
    }
}