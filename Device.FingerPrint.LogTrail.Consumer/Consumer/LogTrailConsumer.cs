namespace Device.FingerPrint.LogTrail.Consumer.Consumer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Device.FingerPrint.Features.LogTrail.Services;
    using Fanex.Logging;
    using FPS.Device.FingerPrint.LogTrail.Net.Standard.Client.Models;
    using MassTransit;

    public class LogTrailConsumer : IConsumer<Batch<LogEventInfo>>
    {
        private readonly ILogTrailServices logTrailServices;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;

        private List<LogEventInfo> logEventInfos = new List<LogEventInfo>();

        public LogTrailConsumer(
            ILogTrailServices logTrailServices,
            IAppSettings appSettings,
            ILogger logger)
        {
            this.logTrailServices = logTrailServices;
            this.appSettings = appSettings;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<Batch<LogEventInfo>> context)
        {
            try
            {
                if (appSettings.GetValue<bool>("ConsumerConfig:LogTrail:Queue-Consume"))
                {
                    logger.Info("Start Consuming!");

                    for (int i = 0; i < context.Message.Length; i++)
                    {
                        LogEventInfo logEvent = context.Message[i].Message;

                        if (logEvent != null)
                        {
                            logEventInfos.Add(logEvent);

                            if (logEventInfos.Count >= appSettings.GetValue<int>("ConsumerConfig:LogTrail:TransBatch"))
                            {
                                var groupLogEvents = logEventInfos
                                .GroupBy(x => new
                                {
                                    x.EventDate.Date,
                                    x.SubscriberID,
                                    x.SystemName,
                                    x.Source,
                                    x.Destination,
                                    x.Method,
                                })
                                .Select(group => new LogEventInfo
                                {
                                    EventDate = group.Key.Date.Date,
                                    SubscriberID = group.Key.SubscriberID,
                                    SystemName = group.Key.SystemName,
                                    Source = group.Key.Source,
                                    Destination = group.Key.Destination,
                                    Method = group.Key.Method,
                                    Total = group.Count()
                                });

                                await InserLogTrailEventAsync(groupLogEvents);

                                logEventInfos.Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }

        public async Task<int> InserLogTrailEventAsync(IEnumerable<LogEventInfo> logEventInfos)
        {
            var rawLogEvent = JsonSerializer.Serialize(logEventInfos);
            return await logTrailServices.InsertSummaryLogTrailAsync(rawLogEvent);
        }
    }
}
