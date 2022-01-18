using System;
using System.Collections.Generic;
using System.Threading;
using Confluent.Kafka;
using Device.FingerPrint.Kafka.Ultilities;
using Microsoft.Extensions.Configuration;
using Sentry;

namespace Device.FingerPrint.Kafka.Handlers
{
    public delegate void CallBack<T>(List<T> list);
    public interface IRequestTimeConsumer
    {
        public void StartConsumerLoop<T>(CancellationToken cancellationToken, CallBack<T> callback);
    }
    public class RequestTimeConsumer : IRequestTimeConsumer
    {
        private readonly string topic;
        private readonly int numberOfRecordsProcessIn1Time;
        private readonly IConsumer<Ignore, string> kafkaConsumer;
        private readonly ISentryClient sentryClient;

        public RequestTimeConsumer(IConfiguration config, ISentryClient sentryClient)
        {
            this.sentryClient = sentryClient;

            var consumerConfig = new ConsumerConfig();
            config.GetSection("KafkaConfig:ConsumerSettings").Bind(consumerConfig);
            topic = config.GetValue<string>("KafkaConfig:TopicName");
            numberOfRecordsProcessIn1Time = config.GetValue<int>("KafkaConfig:NumberOfRecordsProcessIn1Time");
            kafkaConsumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        }

        public void StartConsumerLoop<T>(CancellationToken cancellationToken, CallBack<T> callback)
        {
            kafkaConsumer.Subscribe(topic);

            int count = 0;
            var listEntity = new List<T>();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                { 
                    var consumer = this.kafkaConsumer.Consume(cancellationToken);
                    var value = consumer.Message.Value;
                    var entity = SerializationHelper.DeserializeJson<T>(value);
                    count++;
                    listEntity.Add(entity);
                    if (count == numberOfRecordsProcessIn1Time)
                    {
                        callback(listEntity);
                        kafkaConsumer.Commit(consumer);
                        count = 0;
                        listEntity = new List<T>();
                    }
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                    break;
                }
            }
        }
    }
}
