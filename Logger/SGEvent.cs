namespace Logger
{
    /// <summary>
    /// Type of <seealso cref="SGEvent"/>.
    /// </summary>
    public enum SGEventType
    {
        /// <summary>
        /// Info event type.
        /// </summary>
        Info,
        /// <summary>
        /// Warning event type.
        /// </summary>
        Warning,
        /// <summary>
        /// Error event type.
        /// </summary>
        Error,
        /// <summary>
        /// Abort event type.
        /// </summary>
        Abort
    }
    /// <summary>
    /// The event stored in the logger.
    /// </summary>
    public class SGEvent
    {
        /// <summary>
        /// Timestamp of the event.
        /// </summary>
        public readonly DateTime Timestamp;
        /// <summary>
        /// Type of event. See <seealso cref="EventType"/>.
        /// </summary>
        public readonly SGEventType EventType;
        /// <summary>
        /// The name of the event.
        /// </summary>
        public readonly string EventName;
        /// <summary>
        /// The message of the event.
        /// </summary>
        public readonly string Message;
        /// <summary>
        /// The time elapsed since the logger initialization in milliseconds.
        /// </summary>
        public readonly long ElapsedMs;

        /// <summary>
        /// A log event. Timestamp of the event is assigned in the moment this function is called.
        /// </summary>
        /// <param name="eventType">The type of event</param>
        /// <param name="eventName">The name of the event</param>
        /// <param name="message">The event message</param>
        /// <param name="elapsedMs">Elapsed time in ms since logger initialization.</param>
        public SGEvent(SGEventType eventType, string eventName, string message, long elapsedMs)
        {
            Timestamp = DateTime.Now;
            EventType = eventType;
            EventName = eventName;
            Message = message;
            ElapsedMs = elapsedMs;
        }

        /// <summary>
        /// Return a formatted <seealso cref="SGEvent"/>:
        /// <c>
        /// [<seealso cref="EventType"/>][<seealso cref="Timestamp"/>][<seealso cref="EventName"/>][<seealso cref="ElapsedMs"/>]: <seealso cref="Message"/>/>
        /// </c>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{EventType.ToString().ToUpper()}][{Timestamp}][{EventName}][{ElapsedMs}]: {Message}";
        }
    }
}
