using System.Diagnostics;
namespace Logger
{
    /// <summary>
    /// A simple logger to keep track of events.
    /// </summary>
    public static class SGLogger
    {
        private static Dictionary<Guid, SGEvent> logEvents = new Dictionary<Guid, SGEvent>();
        private static object logEventLock = new object();
        private static Stopwatch stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Add an event to the log.
        /// </summary>
        /// <param name="eventType">Type of event. See <see cref="SGEventType"/>.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="message">The message of the event.</param>
        /// <returns>The GUID of the stored event.</returns>
        public static Guid AddLogEvent(SGEventType eventType, string eventName, string message)
        {
            Guid guid = Guid.NewGuid();
            lock (logEventLock)
            {
                logEvents.Add(guid, new SGEvent(eventType, eventName, message, stopwatch.ElapsedMilliseconds));
            }
            return guid;
        }

        /// <summary>
        /// Returns the log event with the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID of the message.</param>
        /// <returns>The log event.</returns>
        public static SGEvent GetMessage(Guid guid)
        {
            try
            {
                return logEvents[guid];
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guid {guid} was not present in the log event list.");
            }
        }

        /// <summary>
        /// Create a txt file asynchronously with all the log events.
        /// The file line format:
        /// <code>
        /// [GUID]\t\t[TIMESTAMP]\t\t[EVENT TYPE]\t\t[EVENT NAME]\t\t[MESSAGE]\t\t[ELAPSED MS SINCE LOG INITIALIZATION]
        /// </code>
        /// </summary>
        /// <param name="path">Path in which the file is created.</param>
        /// <returns>The task on which the file creation is being processed.</returns>
        public static Task DownloadLogToTxtAsync(string path)
        {
            if (logEvents.Count == 0)
                throw new Exception("Log has no events! Before the download make sure to add events to the logger.");

            return Task.Run(async () =>
            {
                string[] fileLines = new string[logEvents.Count];
                int i = 0;
                foreach (KeyValuePair<Guid, SGEvent> e in logEvents)
                {
                    fileLines[i] = $"{e.Key}\t\t{e.Value.Timestamp}\t\t{e.Value.EventType}\t\t{e.Value.EventName}\t\t{e.Value.Message}\t\t{e.Value.ElapsedMs}";
                    i++;
                }

                await File.WriteAllLinesAsync(path, fileLines);
            });
        }

        /// <summary>
        /// Create a txt file synchronously with all the log events.
        /// The file line format:
        /// <code>
        /// [GUID]\t\t[TIMESTAMP]\t\t[EVENT TYPE]\t\t[EVENT NAME]\t\t[MESSAGE]\t\t[ELAPSED MS SINCE LOG INITIALIZATION]
        /// </code>
        /// </summary>
        /// <param name="path">Path in which the file is created.</param>
        public static void DownloadLogToTxt(string path)
        {
            if (logEvents.Count == 0)
                throw new Exception("Log has no events! Before the download make sure to add events to the logger.");

            string[] fileLines = new string[logEvents.Count];
            int i = 0;
            foreach (KeyValuePair<Guid, SGEvent> e in logEvents)
            {
                fileLines[i] = $"{e.Key}\t\t{e.Value.Timestamp}\t\t{e.Value.EventType}\t\t{e.Value.EventName}\t\t{e.Value.Message}\t\t{e.Value.ElapsedMs}";
                i++;
            }
            File.WriteAllLines(path, fileLines);
        }
        /// <summary>
        /// Logs the event log to the console.
        /// </summary>
        /// <param name="guid">Guid of the log event</param>
        public static void LogToConsole(Guid guid)
        {
            SGEvent e = logEvents[guid];
            ConsoleColor color;

            switch (e.EventType)
            {
                case SGEventType.Info:
                    color = ConsoleColor.Cyan;
                    break;
                case SGEventType.Warning:
                    color = ConsoleColor.Yellow;
                    break;
                case SGEventType.Error:
                    color = ConsoleColor.Red;
                    break;
                case SGEventType.Abort:
                    color = ConsoleColor.DarkRed;
                    break;
                default:
                    color = ConsoleColor.White;
                    break;
            }

            Console.ForegroundColor = color;
            Console.WriteLine(e);
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// Trace the event in the output console.
        /// </summary>
        /// <param name="guid">Guid of the log event</param>
        public static void TraceEvent(Guid guid)
        {
            SGEvent e = logEvents[guid];

            switch (e.EventType)
            {
                case SGEventType.Info:
                    Trace.TraceInformation(e.ToString());
                    break;
                case SGEventType.Warning:
                    Trace.TraceWarning(e.ToString());
                    break;
                case SGEventType.Error:
                    Trace.TraceError(e.ToString());
                    break;
                case SGEventType.Abort:
                    Trace.TraceError(e.ToString());
                    break;
                default:
                    Trace.WriteLine(e.ToString());
                    break;
            }

        }
    }
}
