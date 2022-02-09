using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Device.FingerPrint.Kafka.Handlers.Kafka.Handlers
{
    /// <summary>
    /// Provides mechanism to create Kafka Handler
    /// </summary>
    /// <typeparam name="Tk">Indicates the message's key for Kafka Topic</typeparam>
    /// <typeparam name="Tv">Indicates the message's value for Kafka Topic</typeparam>
    public interface IKafkaHandler<Tk, Tv>
    {
        /// <summary>
        /// Provide mechanism to handle the consumer message from Kafka
        /// </summary>
        /// <param name="key">Indicates the message's key for Kafka Topic</param>
        /// <param name="value">Indicates the message's value for Kafka Topic</param>
        /// <returns></returns>
        Task HandleAsync(Tk key, Tv value);
    }

    /// <summary>
    ///  Provides mechanism to create Kafka Consumer
    /// </summary>
    public interface IKafkaConsumer<TKey, TValue> where TValue : class
    {
        /// <summary>
        ///  Triggered when the service is ready to consume the Kafka topic.
        /// </summary>
        /// <param name="topic">Indicates the message's key for Kafka Topic</param>
        /// <param name="stoppingToken">Indicates cancellation token</param>
        /// <returns></returns>
        Task Consume(string topic, CancellationToken stoppingToken);

        /// <summary>
        /// This will close the consumer, commit offsets and leave the group cleanly.
        /// </summary>
        void Close();
        /// <summary>
        /// Releases all resources used by the current instance of the consumer
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// Base class for implementing Kafka Consumer.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
    {
        private readonly ConsumerConfig _config;
        private IKafkaHandler<TKey, TValue> _handler;
        private IConsumer<TKey, TValue> _consumer;
        private string _topic;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Indicates constructor to initialize the serviceScopeFactory and ConsumerConfig
        /// </summary>
        /// <param name="config">Indicates the consumer configuration</param>
        /// <param name="serviceScopeFactory">Indicates the instance for serviceScopeFactory</param>
        public KafkaConsumer(ConsumerConfig config, IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _config = config;
        }

        /// <summary>
        /// Triggered when the service is ready to consume the Kafka topic.
        /// </summary>
        /// <param name="topic">Indicates Kafka Topic</param>
        /// <param name="stoppingToken">Indicates stopping token</param>
        /// <returns></returns>
        public async Task Consume(string topic, CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            _handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
            _consumer = new ConsumerBuilder<TKey, TValue>(_config).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
            _topic = topic;

            await Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        /// <summary>
        /// This will close the consumer, commit offsets and leave the group cleanly.
        /// </summary>
        public void Close()
        {
            _consumer.Close();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the consumer
        /// </summary>
        public void Dispose()
        {
            _consumer.Dispose();
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);

                    if (result != null)
                    {
                        await _handler.HandleAsync(result.Message.Key, result.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    // Consumer errors should generally be ignored (or logged) unless fatal.
                    Console.WriteLine($"Consume error: {e.Error.Reason}");

                    if (e.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error: {e}");
                    break;
                }
            }
        }
    }
}
