namespace Microsoft.Mixer
{
    /// <summary>
    /// Enum representing the current state of the interactivity service.
    /// The state transitions are:
    /// NotInitialized -> Initializing
    /// Initializing -> ShortCodeRequired
    /// ShortCodeRequired -> InteractivityPending
    /// InteractivityPending -> InteractivityEnabled
    /// InteractivityEnabled -> InteractivityDisabled
    /// </summary>
    public enum InteractivityState
    {
        /// <summary>
        /// The InteractivityManager is not initalized.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// The InteractivityManager is in the process of initializing.
        /// </summary>
        Initializing,

        /// <summary>
        /// The InteractivityManager needs the user to enter a short code on the website.
        /// in order to authenticate with the service.
        /// </summary>
        ShortCodeRequired,

        /// <summary>
        /// The InteractivityManager is initialized.
        /// </summary>
        Initialized,

        /// <summary>
        /// The InteractivityManager is initialized, but interactivity is not enabled.
        /// </summary>
        InteractivityDisabled,

        /// <summary>
        /// Currently connecting to the interactivity service.
        /// </summary>
        InteractivityPending,

        /// <summary>
        /// Interactivity enabled
        /// </summary>
        InteractivityEnabled
    }
}
