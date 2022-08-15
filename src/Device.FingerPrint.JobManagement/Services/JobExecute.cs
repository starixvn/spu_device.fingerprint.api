namespace Device.FingerPrint.JobManagement.Services
{
    using Fanex.Logging;
    using ONEbook.UM.Services;
    using Quartz;
    using System;
    using System.Threading.Tasks;

    [DisallowConcurrentExecution, PersistJobDataAfterExecution]
    public class JobExecute : IJob
    {
        private readonly IJobService jobService;
        private readonly IScheduler scheduler;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IAppSettings _appSettings;

        public JobExecute(IJobService jobService
            , ISchedulerFactory schedulerFactory
            , IMaintenanceService maintenanceService
            , IAppSettings appSettings)
        {
            this.jobService = jobService;
            scheduler = schedulerFactory.GetScheduler().Result;
            _maintenanceService = maintenanceService;
            _appSettings = appSettings;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var isUM = _maintenanceService.GetUnderMaintenanceTime().IsUnderMaintenanceTime;

            if (!isUM)
            {
                var dataMap = context.JobDetail.JobDataMap;
                var jobName = context.JobDetail.Key.Name;
                var apiUrl = dataMap.GetString("apiUrl");
                var count = dataMap.GetInt("count");
                var methodName = dataMap.GetString("methodName");
                var controller = dataMap.GetString("controller");

                if (count >= 5)
                {
                    var e = new JobExecutionException("Retries exceeded");

                    await scheduler.PauseTrigger(context.Trigger.Key);

                    dataMap.PutAsString("count", 0);

                    LogService.Error($"{jobName} is STOPPED. Cause of RETRIED EXECEEDED.");

                    throw e;
                }

                try
                {
                    var queryString = JobHelper.GetQueryData(apiUrl);

                    var url = JobHelper.GetUrl(apiUrl);

                    var result = jobService.Execute(jobName, url, methodName, controller, queryString);

                    if (!result)
                    {
                        count++;

                        dataMap.PutAsString("count", count);
                        var errorMessage = $"{jobName} is failed at {DateTime.Now}.";

                        throw JobHelper.JobExecuteFailed(errorMessage);
                    }
                    else
                    {
                        dataMap.PutAsString("count", 0);
                    }
                }
                catch (Exception ex)
                {
                    count++;

                    dataMap.PutAsString("count", count);

                    throw JobHelper.JobExecuteFailed(ex.Message);
                }
            }
        }
    }
}
