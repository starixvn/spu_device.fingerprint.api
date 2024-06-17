using System;
using System.Collections.Generic;
using System.Text;

namespace FPS.Client.NetStandard.Enum
{
    /// <summary>
    /// Logging Mode.
    /// </summary>
    internal enum LogMode
    {
        /// <summary>
        /// Do not log.
        /// </summary>
        NoLog = 0,

        /// <summary>
        /// New log file created daily.
        /// </summary>
        Daily = 1,

        /// <summary>
        /// Write log to one file persistently.
        /// </summary>
        Persist = 2
    }
}
