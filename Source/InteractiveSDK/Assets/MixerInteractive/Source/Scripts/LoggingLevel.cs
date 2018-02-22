namespace Microsoft.Mixer
{
    /// <summary>
    /// A list of possible levels of logging from the Interactive SDK.
    /// </summary>
    public enum LoggingLevel
    {
        /// <summary>
        /// No debug output.
        /// </summary>
        None,

        /// <summary>
        /// Only errors and warnings.
        /// </summary>
        Minimal,

        /// <summary>
        /// All events, including every websocket and HTTP message.
        /// </summary>
        Verbose
    }
}
