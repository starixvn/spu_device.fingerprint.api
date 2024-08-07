namespace FPS.Client.NetStandard.Settings
{
    internal struct DefaultValues
    {
        #region Detector.cs

        /// <summary>
        /// The minimum number of requests on which estimation is based to calculate current down-rate.
        /// </summary>
        public const int FPSMinDeterminativeRequest = 10;

        /// <summary>
        /// The maximum number of failures allowed within fpsMonitorTime.
        /// </summary>
        public const int FPSIntervalMaxFailed = int.MaxValue;

        /// <summary>
        /// Maximum rate of failed requests over total requests sent for service calls to continue.
        /// </summary>
        public const double FPSIntervalMaxDownRate = 0.5;

        /// <summary>
        /// Time (in minutes) for service's down-mark removal.
        /// </summary>
        public const double FPSResumeTime = 60.0;

        /// <summary>
        /// The number of milliseconds when Detector re-checks 
        /// service condition if new requests come.
        /// </summary>
        public const int FPSCheckServiceAcceptCallsInterval = 2;

        #endregion

        #region RequestMementoPool.cs

        /// <summary>
        /// The amount of time (in minutes) in which all latest requests sent are monitored
        /// to decide web service condition (running or down).
        /// </summary>
        public const double FPSMonitorTime = 2.0;

        /// <summary>
        /// The estimated number of transactions accepted in one second,
        /// used to determine FPSMaxRequestMonitored if no user's 
        /// config value is found.
        /// </summary>
        public const int FPSEstimatedTransactionPerSecond = 100;

        #endregion

        #region RequestManager.cs

        /// <summary>
        /// The maximum number of requests pending in asynchronous calls. If this 
        /// amount is reached, no more asynchronous requests are sent. The RequestManager
        /// will wait and check until there're less pending in request queue.
        /// </summary>
        public const int FPSMaxPendingRequest = 200;

        #endregion

        #region Service.cs

        /// <summary>
        /// Timeout setting, indicating the time (in millisecond) for web service
        /// clients waits for the reply from web service server upon a request.
        /// </summary>
        public const int FPSRequestTimeout = 3000;

        #endregion
    }
}
