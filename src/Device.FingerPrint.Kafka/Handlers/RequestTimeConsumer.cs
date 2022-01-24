using Confluent.Kafka;
using Device.FingerPrint.Kafka.Ultilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;

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

        public RequestTimeConsumer(IConfiguration config)
        {            
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
                var consumer = this.kafkaConsumer.Consume(cancellationToken);
                if (!string.IsNullOrWhiteSpace(consumer.Message.Value))
                {
                    Console.WriteLine($"{consumer.Message.Key}: {consumer.Message.Value}ms");
                    Console.WriteLine($"TopicPartitionOffset: {consumer.TopicPartitionOffset}");
                    var entity = SerializationHelper.DeserializeJson<T>(consumer.Message.Value);
                    count++;
                    listEntity.Add(entity);

                    if (count >= numberOfRecordsProcessIn1Time)
                    {
                        callback(listEntity);

                        kafkaConsumer.Commit(consumer);

                        count = 0;
                        listEntity.Clear();
                    }
                }
            }
        }
    }
}
