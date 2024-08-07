namespace FPS.Client.NetStandard.Settings
{
    internal struct ConfigurationKeys
    {
        #region Detector.cs

        /// <summary>
        /// The maximum number of failures allowed within fpsMonitorTime.
        /// </summary>
        public const string FPSIntervalMaxFailed = "FPS_Interval_MaxFailed";

        /// <summary>
        /// Maximum rate of failed requests over total requests sent for service calls to continue.
        /// </summary>
        public const string FPSIntervalMaxDownRate = "FPS_Interval_MaxDownRate";

        /// <summary>
        /// The minimum number of requests on which estimation is based to calculate current down-rate.
        /// </summary>
        public const string FPSMinDeterminativeRequest = "FPS_Interval_MinDeRequest";

        /// <summary>
        /// Time (in minutes) for service's down-mark removal.
        /// </summary>
        public const string FPSResumeTime = "FPS_ResumeTime";

        #endregion

        #region RequestMementoPool.cs

        /// <summary>
        /// The amount of time (in minutes) in which all latest requests sent are monitored
        /// to decide web service condition (running or down).
        /// </summary>
        public const string FPSMonitorTime = "FPS_MonitorTime";

        /// <summary>
        /// The maximum number of requests allowed within 1 second.
        /// </summary>
        public const string FPSMaxReqPerSecond = "FPS_MaxReqPerSecond";

        /// <summary>
        /// The maximum number of requests (async and sync) sent in the last [FPSMonitorTime]
        /// (e.g: in the last 2 minutes, there must be no more than 600 requests are sent).
        /// Don't care about the requests status (succeeded, failed, timed out...). 
        /// Once reached, requests will not be sent.
        /// </summary>
        public const string FPSMaxRequestMonitored = "FPS_MaxRequestMonitored";

        #endregion

        #region Logger.cs

        public const string FPSLog = "FPS_Log";

        #endregion

        #region RequestManager.cs

        /// <summary>
        /// The maximum number of requests pending in asynchronous calls. If this 
        /// amount is reached, no more asynchronous requests are sent. The RequestManager
        /// will wait and check until there're less pending in request queue.
        /// </summary>
        public const string FPSMaxPendingRequest = "FPS_MaxPendingRequest";

        #endregion

        #region Service.cs

        /// <summary>
        /// The FPS request timeout.
        /// </summary>
        public const string FPSRequestTimeout = "FPS_RequestTimeout";

        #endregion
    }
}
