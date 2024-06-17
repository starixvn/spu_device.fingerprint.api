using System;
using System.Collections.Generic;
using System.Text;

namespace FPS.Client.NetStandard.LogWriter
{
    using System;

    /// <summary>
    /// Class LogMessage.
    /// </summary>
    internal class LogMessage
    {
        public DateTime LogTime { get; set; }

        public string[] Messages { get; set; }
    }
}
