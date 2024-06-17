using System;
using System.Collections.Generic;
using System.Text;

namespace FPS.Client.NetStandard.LogWriter
{
    using System;
    using System.Configuration;
    using System.IO;
    using FPS.Client.NetStandard.Enum;
    using FPS.Client.NetStandard.Settings;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    internal class Logger
    {
        #region Private vars

        private const int ITEMCOUNT = 10; // count down in 10 log items
        private const int TIMERANGE = 5; // in seconds

        private static readonly string[] LogComponents = new string[]
                                                        {
                                                            "FPS called {0} at {1}",
                                                            Environment.NewLine,
                                                            "Result Code: {2}",
                                                            Environment.NewLine,
                                                            "Params: {3}",
                                                            Environment.NewLine,
                                                            "Result message: {4}",
                                                            Environment.NewLine
                                                        };

        private static readonly string LogPath;
        private static readonly LogMode LogMode = LogMode.NoLog;
        private static readonly string LogFormat = string.Join(null, LogComponents);
        private static LogQueue<LogMessage> logQueue;
        private static object lockPad = new object();
        private static DateTime recentWrite = DateTime.MinValue;

        #endregion

        #region Constructor

        static Logger()
        {
            try
            {
                string logPath = "/Logs";
                if (!string.IsNullOrEmpty(logPath))
                {
                    logPath = logPath.TrimEnd('\\', '/').TrimStart('\\', '/');

                    if (!Path.IsPathRooted(logPath))
                    {
                        // Relative path => Absolute one
                        string root = AppDomain.CurrentDomain.BaseDirectory;

                        logPath = Path.GetFullPath(Path.Combine(root, logPath));
                    }

                    Logger.LogPath = logPath;

                    bool isDirectory = Logger.IsDirectory(Logger.LogPath);

                    if (isDirectory)
                    {
                        // Create a new log file for each day
                        Logger.LogMode = LogMode.Daily;
                    }
                    else
                    {
                        // Persist the log to the same file in configuration
                        Logger.LogMode = LogMode.Persist;
                    }

                    // Init log queue
                    logQueue = new LogQueue<LogMessage>();
                    logQueue.OnEnqueue += new EventHandler(LogOnEvent);
                }
            }
            catch
            {
                /**ignore catch**/
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Writes the log.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteLog(params string[] message)
        {
            if (Logger.LogMode != LogMode.NoLog
                && message != null)
            {
                LogMessage logItem = new LogMessage
                {
                    LogTime = DateTime.Now,
                    Messages = message
                };
                logQueue.Enqueue(logItem);
            }
        }
        #endregion

        #region Private methods

        /// <summary>
        /// On Log event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void LogOnEvent(object sender, EventArgs e)
        {
            if (sender != null)
            {
                try
                {
                    DateTime logTime = DateTime.MinValue;
                    bool canWrite = false, writeTime = false, writeCount = false;
                    if (DateTime.TryParse(sender.ToString(), out logTime))
                    {
                        writeTime = logTime.Subtract(recentWrite).TotalSeconds > TIMERANGE;
                    }

                    writeCount = logQueue.Count >= ITEMCOUNT;
                    canWrite = writeTime || (writeCount && writeTime);
                    if (canWrite)
                    {
                        LogMessage[] logList;
                        lock (logQueue)
                        {
                            logList = logQueue.ToArray();
                            logQueue.Clear();
                        }

                        LogOnWrite(logList);
                    }
                }
                catch
                {
                    /**inogre catch**/
                }
            }
        }

        /// <summary>
        /// On log write event.
        /// </summary>
        /// <param name="logList">The log list.</param>
        private static void LogOnWrite(LogMessage[] logList)
        {
            if (logList != null
                && logList.Length > 0)
            {
                string logFile = string.Empty;

                switch (Logger.LogMode)
                {
                    case LogMode.Daily:
                        logFile = string.Join(null, new string[] { Logger.LogPath, "/", "FPS_CLIENT_API_LOG_", DateTime.Now.ToString("yyyyMMdd"), ".log" });
                        break;
                    case LogMode.Persist:
                        logFile = Logger.LogPath;
                        break;
                }

                if (!string.IsNullOrEmpty(logFile)
                    && Logger.LogMode != LogMode.NoLog)
                {
                    try
                    {
                        lock (lockPad)
                        {
                            using (FileStream fs = new FileStream(logFile, FileMode.OpenOrCreate | FileMode.Append, FileAccess.Write))
                            {
                                using (StreamWriter writer = new StreamWriter(fs))
                                {
                                    string log = string.Empty;
                                    foreach (LogMessage logItem in logList)
                                    {
                                        log += string.Format(Logger.LogFormat, logItem.Messages);
                                    }

                                    writer.WriteLine(log);
                                }
                            }

                            // replace recent write
                            recentWrite = DateTime.Now;
                        }
                    }
                    catch
                    {
                        /**ignore catch**/
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified path is directory.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the specified path is directory; otherwise, <c>false</c>.</returns>
        private static bool IsDirectory(string path)
        {
            bool isDirectory = false;

            try
            {
                FileAttributes fa = File.GetAttributes(path);

                if ((fa & FileAttributes.Directory) != 0)
                {
                    isDirectory = true;
                }
            }
            catch
            {
                /**ignore catch**/
            }

            return isDirectory;
        }
        #endregion
    }
}
