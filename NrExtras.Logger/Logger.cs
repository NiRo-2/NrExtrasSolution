using System.Diagnostics;

namespace NrExtras.Logger
{
    public static class Logger
    {
        //helper enum for setting log event type
        public enum LogLevel
        {
            Trace = 0, Debug = 1, Information = 2, Warning = 3, Error = 4, Critical = 5
        };

        /// <summary>
        /// Get this app name from global appDomain
        /// </summary>
        /// <returns></returns>
        public static string GetAppName()
        {
            return AppDomain.CurrentDomain.FriendlyName;
        }

        /// <summary>
        /// Write string to console and event viewer using event type
        /// Works on Windows OS only
        /// </summary>
        /// <param name="s">value to write</param>
        /// <param name="logLevel">event type. information by default</param>
        /// <param name="writeToLogger">true by default. write to external logger. for now we use windows event viewer</param>
        public static void WriteToLog(string s, LogLevel logLevel = LogLevel.Information, bool writeToLogger = true)
        {
            //write to event viewer
            if (writeToLogger) WriteToWindowsEventLog(s, logLevel);

            //write to console
            if (logLevel == LogLevel.Error)
                WriteToConsole(s, true, false);
            else
                WriteToConsole(s, false, false);
        }
        /// <summary>
        /// Write exception to console and log
        /// </summary>
        /// <param name="exception">exception to write</param>
        public static void WriteToLog(Exception exception)
        {
            WriteToLog("Error: " + exception, LogLevel.Error);
        }
        /// <summary>
        /// Write to console with option to add date and time
        /// </summary>
        /// <param name="s">string to write</param>
        /// <param name="isError">false by default. if true, write to console as error</param>
        /// <param name="writeDateAndTime">true by default. if true, add date and time prefix</param>
        public static void WriteToConsole(string s, bool isError = false, bool writeDateAndTime = true)
        {
            string msg = "";
            //add time prefix
            if (writeDateAndTime)
                msg = DateTime.Now.ToString() + " - " + s;
            else
                msg = s;

            //incase this is an error
            if (isError)
                Console.Error.WriteLine(msg);
            else
                Console.WriteLine(msg);
        }

        /// <summary>
        /// Write data to windows event viewer
        /// overload function to convert NrExtras LogLevel to windows event viewer entry type
        /// </summary>
        /// <param name="s">message to write</param>
        /// <param name="logLevel">log level. default is information</param>
        public static void WriteToWindowsEventLog(string s, LogLevel logLevel = LogLevel.Information)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    WriteToWindowsEventLog(s, EventLogEntryType.Information);
                    break;
                case LogLevel.Warning:
                    WriteToWindowsEventLog(s, EventLogEntryType.Warning);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    WriteToWindowsEventLog(s, EventLogEntryType.Error);
                    break;
            }
        }
        /// <summary>
        /// Write data to windows event viewer
        /// </summary>
        /// <param name="s">message to write</param>
        /// <param name="eventLogEntryType">event type. default is information</param>
        public static void WriteToWindowsEventLog(string s, EventLogEntryType eventLogEntryType = EventLogEntryType.Information)
        {
            try
            {
                EventLog eventLog = new EventLog() { Source = GetAppName() };
                eventLog.WriteEntry(s, eventLogEntryType);
            }
            catch (Exception ex)
            {
                WriteToLog("Error writing data to windows event viewer", LogLevel.Error);
                WriteToLog(ex);
            }
        }
    }
}