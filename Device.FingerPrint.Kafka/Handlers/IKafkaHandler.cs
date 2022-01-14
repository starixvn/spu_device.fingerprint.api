using Confluent.Kafka;
using Device.FingerPrint.Kafka.Ultilities;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using Microsoft.Extensions.Hosting;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.FingerPrint.Kafka.Handlers
{
    public delegate void CallBack<T>(List<T> list);

    public interface IKafkaHandler
    {
        public Task StartAsync<T>(CancellationToken cancellationToken, IAppSettings appSettings, CallBack<T> callback);
        public Task StopAsync(CancellationToken cancellationToken);
    }

    public class KafkaHandler : IKafkaHandler
    {
        private readonly ISentryClient sentryClient;

        public Task StartAsync<T>(CancellationToken cancellationToken, IAppSettings appSettings, CallBack<T> callback)
        {
            var conf = new ConsumerConfig
            {
                GroupId = appSettings.ConsumerGroupName,
                BootstrapServers = appSettings.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
            using (var builder = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                builder.Subscribe(appSettings.TopicName);
                var cancelToken = new CancellationTokenSource();
                try
                {
                    int count = 0;
                    var listFingerPrint = new List<T>();
                    while (true)
                    {
                        var consumer = builder.Consume(cancelToken.Token);
                        var value = consumer.Message.Value;
                        var fingerPrintEntity = SerializationHelper.DeserializeJson<T>(value);
                        count++;
                        listFingerPrint.Add(fingerPrintEntity);
                        if (count == int.Parse(appSettings.NumberOfRecordsProcessIn1Time))
                        {
                            callback(listFingerPrint);
                            builder.Commit(consumer);
                            count = 0;
                            listFingerPrint = new List<T>();
                        }
                    }
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
