namespace Device.FingerPrint.JobManagement.Services
{
    using Fanex.Logging;
    using Fanex.NAP.SPU.Notification.Services;
    using ONEbook.UM.Services;
    using Quartz;
    using Quartz.Impl.Matchers;
    using Sentry;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [DisallowConcurrentExecution, PersistJobDataAfterExecution]
    public class JobMonitoring : IJob
    {
        private readonly IScheduler _scheduler;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IAppSettings _appSettings;

        public JobMonitoring(IAppSettings appSettings, ISchedulerFactory schedulerFactory, IMaintenanceService maintenanceService)
        {
            _scheduler = schedulerFactory.GetScheduler().Result;
            _maintenanceService = maintenanceService;
            _appSettings = appSettings;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var isUM = _maintenanceService.GetUnderMaintenanceTime().IsUnderMaintenanceTime;

                if (!isUM)
                {
                    var allTriggerKeys = _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
                    var result = new List<string>();

                    foreach (var triggerKey in allTriggerKeys.Result)
                    {
                        var triggerdetails = await _scheduler.GetTrigger(triggerKey);
                        var state = await _scheduler.GetTriggerState(triggerdetails.Key);

                        if (state == TriggerState.Paused)
                        {
                            result.Add(triggerdetails.JobKey.Name);
                        }
                    }

                    if (result.Any())
                    {
                        var jobName = string.Join("", result.OrderBy(x => x.ToString()).Select(x => "\n\r - " + x));

                        Notification("Jobs Monitoring", "The JOBS are PAUSED.", "The List of jobs. \n\r" + jobName);
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }
        }

        private void Notification(string jobName, string message, string moreInfo)
        {
            var notification = new NotificationService();

            notification
                      .Config("0A40aOc7CtpzjbBSJu8KNWQRoXyjRvf9", "-506613866", "https://bot.nexdev.net/skynex/api/messages/forwardwithtoken")
                      .Message(new MessageModel(jobName, DateTime.UtcNow.AddHours(7), _appSettings.Environment, message, moreInfo))
                      .SendWithDelay(300);
        }
    }
}
