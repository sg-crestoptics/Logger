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

        private static Dictionary<Guid, SGEvent> startEvents = new Dictionary<Guid, SGEvent>();
        private static object startEventLock = new object();

        private static Dictionary<Guid, SGEvent> stopEvents = new Dictionary<Guid, SGEvent>();
        private static object stopEventLock = new object();

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
                try
                {
                    logEvents.Add(guid, new SGEvent(eventType, eventName, message, stopwatch.ElapsedMilliseconds));

                }
                catch (Exception)
                {
                    throw new Exception($"Problem adding this event:\n[{eventType.ToString().ToUpper()}][{eventName}] {message}");
                }
            }
            return guid;
        }

        /// <summary>
        /// Add an event to the log.
        /// </summary>
        /// <param name="eventType">Type of event. See <see cref="SGEventType"/>.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="message">The message of the event.</param>
        /// <returns>The GUID of the stored event.</returns>
        public static Guid AddEventStart(SGEventType eventType, string eventName, string message)
        {
            Guid guid = Guid.NewGuid();
            lock (startEventLock)
            {
                try
                {
                    startEvents.Add(guid, new SGEvent(eventType, eventName, message, stopwatch.ElapsedMilliseconds));

                }
                catch (Exception)
                {
                    throw new Exception($"Problem adding this event:\n[{eventType.ToString().ToUpper()}][{eventName}] {message}");
                }
            }
            return guid;
        }

        /// <summary>
        /// Add the stop event.
        /// </summary>
        /// <param name="guid">GUID of the event that has to be stopped.</param>
        /// <exception cref="ArgumentException">GUID does not exist or it has not been started yet.</exception>
        public static void AddEventStop(Guid guid)
        {
            try
            {
                SGEvent startEvent = startEvents[guid];

                lock (stopEventLock)
                {
                    try
                    {
                        stopEvents.Add(guid, new SGEvent(startEvent.EventType, startEvent.EventName, startEvent.Message, stopwatch.ElapsedMilliseconds));
                    }
                    catch (ArgumentException)
                    {
                        throw new ArgumentException($"Stop event with GUID {guid} already done.");
                    }
                }
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guid {guid} was not present in the start event list.");
            }
        }

        /// <summary>
        /// Returns the logged event with the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID of the logged event.</param>
        /// <returns>The logged event.</returns>
        /// <exception cref="ArgumentException">Triggered when the GUID specified is not found.</exception>
        public static SGEvent GetEvent(Guid guid)
        {
            try
            {
                // a lock is not necessary since you cannot pass the guid returned from the Add method, and when you pass it you already know it is on the logEvents structure
                return logEvents[guid];
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guid {guid} was not present in the event log.");
            }
        }

        /// <summary>
        /// Returns the logged start event with the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID of the logged event.</param>
        /// <returns>The logged start event.</returns>
        /// <exception cref="ArgumentException">Triggered when the GUID specified is not found.</exception>
        public static SGEvent GetStartEvent(Guid guid)
        {
            try
            {
                // a lock is not necessary since you cannot pass the guid returned from the Add method, and when you pass it you already know it is on the startEvents structure
                return startEvents[guid];
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guid {guid} was not present in the start event log.");
            }
        }

        /// <summary>
        /// Returns the logged stop event with the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID of the logged event.</param>
        /// <returns>The logged stop event.</returns>
        /// <exception cref="ArgumentException">Triggered when the GUID specified is not found.</exception>
        public static SGEvent GetStopEvent(Guid guid)
        {
            try
            {
                // a lock is not necessary since you cannot pass the guid returned from the Add method, and when you pass it you already know it is on the stopEvents structure
                return stopEvents[guid];
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guid {guid} was not present in the start event log.");
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
                string[] fileLines;
                lock (logEventLock)
                {
                    fileLines = new string[logEvents.Count];
                    int i = 0;
                    foreach (KeyValuePair<Guid, SGEvent> e in logEvents)
                    {
                        fileLines[i] = $"{e.Key}\t\t{e.Value.Timestamp}\t\t{e.Value.EventType}\t\t{e.Value.EventName}\t\t{e.Value.Message}\t\t{e.Value.ElapsedMs}";
                        i++;
                    }
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

            string[] fileLines;

            lock (logEventLock)
            {
                fileLines = new string[logEvents.Count];
                int i = 0;
                foreach (KeyValuePair<Guid, SGEvent> e in logEvents)
                {
                    fileLines[i] = $"{e.Key}\t\t{e.Value.Timestamp}\t\t{e.Value.EventType}\t\t{e.Value.EventName}\t\t{e.Value.Message}\t\t{e.Value.ElapsedMs}";
                    i++;
                }
            }

            File.WriteAllLines(path, fileLines);
        }

        /// <summary>
        /// Create a txt file asynchronously with all the persisting events logged.
        /// The file line format:
        /// <code>
        /// [GUID]\t\t[TIMESTAMP]\t\t[EVENT TYPE]\t\t[EVENT NAME]\t\t[MESSAGE]\t\t[ELAPSED MS SINCE LOG INITIALIZATION]
        /// </code>
        /// </summary>
        /// <param name="path">Path in which the file is created.</param>
        /// <returns>The task on which the file creation is being processed.</returns>
        public static Task DownloadPersistingEventsAsync(string path)
        {
            if (startEvents.Count == 0)
                throw new Exception("Log has no events! Before the download make sure to add events to the logger.");

            return Task.Run(async () =>
            {
                string[] fileLines;
                lock (startEventLock)
                {
                    fileLines = new string[startEvents.Count];
                    SGEvent stopEvent;
                    int i = 0;
                    foreach (KeyValuePair<Guid, SGEvent> startEvent in startEvents)
                    {
                        stopEvent = stopEvents[startEvent.Key];
                        fileLines[i] = $"{startEvent.Key}\t\t{startEvent.Value.Timestamp}\t\t{startEvent.Value.EventType}\t\t{startEvent.Value.EventName}\t\t{startEvent.Value.Message}\t\t{startEvent.Value.ElapsedMs}\t\t{stopEvent.ElapsedMs}";
                        i++;
                    }
                }

                await File.WriteAllLinesAsync(path, fileLines);
            });
        }

        /// <summary>
        /// Create a txt file asynchronously with all the persisting events logged.
        /// The file line format:
        /// <code>
        /// [GUID]\t\t[TIMESTAMP]\t\t[EVENT TYPE]\t\t[EVENT NAME]\t\t[MESSAGE]\t\t[ELAPSED MS SINCE LOG INITIALIZATION]
        /// </code>
        /// </summary>
        /// <param name="path">Path in which the file is created.</param>
        public static void DownloadPersistingEventsSync(string path)
        {
            if (logEvents.Count == 0)
                throw new Exception("Log has no events! Before the download make sure to add events to the logger.");

            string[] fileLines;

            lock (startEventLock)
            {
                fileLines = new string[startEvents.Count];
                SGEvent stopEvent;
                int i = 0;
                foreach (KeyValuePair<Guid, SGEvent> startEvent in startEvents)
                {
                    stopEvent = stopEvents[startEvent.Key];
                    fileLines[i] = $"{startEvent.Key}\t\t{startEvent.Value.Timestamp}\t\t{startEvent.Value.EventType}\t\t{startEvent.Value.EventName}\t\t{startEvent.Value.Message}\t\t{startEvent.Value.ElapsedMs}\t\t{stopEvent.ElapsedMs}";
                    i++;
                }
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
