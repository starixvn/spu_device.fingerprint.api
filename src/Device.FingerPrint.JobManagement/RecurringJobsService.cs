using Hangfire;
using Hangfire.Annotations;
using Microsoft.Extensions.Hosting;
using Sentry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.FingerPrint.JobManagement
{

    public class RecurringJobsService : BackgroundService
    {
        private readonly IRecurringJobManager _recurringJobs;
        private readonly ISentryClient _sentryClient;

        
        public RecurringJobsService([NotNull] IRecurringJobManager recurringJobs, ISentryClient sentryClient)
        {
            _recurringJobs = recurringJobs ?? throw new ArgumentNullException(nameof(recurringJobs));
            _sentryClient = sentryClient;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                _recurringJobs.AddOrUpdate("seconds", () => Console.WriteLine("Hello, seconds!"), "*/15 * * * * *");
                _recurringJobs.AddOrUpdate("minutely", () => Console.WriteLine("Hello, world!"), Cron.Minutely);
                _recurringJobs.AddOrUpdate("hourly", () => Console.WriteLine("Hello"), "25 15 * * *");
                _recurringJobs.AddOrUpdate("neverfires", () => Console.WriteLine("Can only be triggered"), "0 0 31 2 *");

                _recurringJobs.AddOrUpdate("Hawaiian", () => Console.WriteLine("Hawaiian"), "15 08 * * *", TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
                _recurringJobs.AddOrUpdate("UTC", () => Console.WriteLine("UTC"), "15 18 * * *");
                _recurringJobs.AddOrUpdate("Russian", () => Console.WriteLine("Russian"), "15 21 * * *", TimeZoneInfo.Local);


            }
            catch (Exception ex)
            {
                _sentryClient.CaptureException(ex);
            }

            return Task.CompletedTask;
        }
    }
}
