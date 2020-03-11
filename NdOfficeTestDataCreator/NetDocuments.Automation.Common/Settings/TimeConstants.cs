namespace NetDocuments.Automation.Common.Settings
{
    /// <summary>
    /// Holds common time constants.
    /// </summary>
    public static class TimeConstants
    {
        /// <summary>
        /// Thread's sleep time, uses in "heavy" functions for CPU offloading.
        /// By default, 50 milliseconds. Can't be zero.
        /// </summary>
        public const int SLEEP_TIME = 50;

        /// <summary>
        /// Maximum waiting time. By default, 60 seconds. Can't be zero.
        /// </summary>
        public const int MAX_WAIT_TIME = 60000;

        /// <summary>
        /// Maximum waiting time for a tooltip. By default, 10 seconds. Can't be zero.
        /// </summary>
        public const int MAX_WAIT_TIME_FOR_TOOLTIP = 10000;

        /// <summary>
        /// Wait time while ndOffice starting.
        /// </summary>
        public const int ND_OFFICE_START_WAIT_TIME = 6000;

        /// <summary>
        /// Wait for separate outlook window.
        /// </summary>
        public static int OUTLOOK_SEPARATE_WINDOW = 3000;

        /// <summary>
        /// Wait for predictions panel.
        /// </summary>
        public static int OUTLOOK_PREDICTIONS_PANEL = 3000;

        /// <summary>
        /// Wait for outlook message table.
        /// </summary>
        public static int OUTLOOK_MESSAGE_TABLE = 30000;

        /// <summary>
        /// Wait for user cabinet on predictions panel.
        /// </summary>
        public static int OUTLOOK_USER_CABINET_PREDICTIONS_PANEL = 100000;

        /// <summary>
        /// No wait time (wait time is 0).
        /// </summary>
        public static int NO_WAIT_TIME = 0;
    }
}
