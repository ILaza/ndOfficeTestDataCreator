namespace NetDocuments.Automation.Common.Components
{
    // TODO: Think about the location of this enum in our solution.
    //       Does it belong here?
    /// <summary>
    /// Represents different states for SettingItem objects.
    /// </summary>
    public enum SettingStates
    {
        /// <summary>
        /// State for selected items.
        /// </summary>
        Selected = 0,

        /// <summary>
        /// State for unselected items.
        /// </summary>
        Unselected = 1,

        /// <summary>
        /// State for items that don't support selection.
        /// </summary>
        NA = 2
    }
}
