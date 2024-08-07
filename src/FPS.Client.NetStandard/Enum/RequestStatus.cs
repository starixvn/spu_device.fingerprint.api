using System;
using System.Collections.Generic;
using System.Text;

namespace FPS.Client.NetStandard.Enum
{
    /// <summary>
    /// Request Status.
    /// </summary>
    internal enum RequestStatus
    {
        /// <summary>
        /// Waiting status.
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// Expired status.
        /// </summary>
        Expired = 1,

        /// <summary>
        /// Failed status.
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Timed out status.
        /// </summary>
        Timedout = 3,

        /// <summary>
        /// Succeeded status.
        /// </summary>
        Succeeded = 4
    }
}
