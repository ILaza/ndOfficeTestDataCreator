namespace NetDocuments.Automation.Common.Entities.Outlook
{
    /// <summary>
    /// Class which keep outlook separate windows titles.
    /// </summary>
    public class OutlookSeparateWindowTitles
    {
        /// <summary>
        /// Separate outlook email window title.
        /// </summary>
        /// <param name="subject">Subject of opened email.</param>
        public string EmailTitle(string subject) => $"{subject} - Message (HTML) ";

        /// <summary>
        /// Separate outlook appointment window title.
        /// </summary>
        /// <param name="subject">Subject of opened appointment.</param
        public string AppointmentTitle(string subject) => $"{subject} - Appointment";

        /// <summary>
        /// Separate outlook meeting window title.
        /// </summary>
        /// <param name="subject">Subject of opened Meeting.</param
        public string MeetingTitle(string subject) => $"{subject} - Meeting";

        /// <summary>
        /// Separate outlook meeting response window title.
        /// </summary>
        /// <param name="subject">Subject of opened meeting response.</param
        public string MeetingResponseTitle(string subject) => $"{subject} - Meeting Response";

        /// <summary>
        /// Separate outlook report window title.
        /// </summary>
        /// <param name="subject">Subject of opened report.</param
        public string ReportTitle(string subject) => $"{subject} - Report";
    }
}
