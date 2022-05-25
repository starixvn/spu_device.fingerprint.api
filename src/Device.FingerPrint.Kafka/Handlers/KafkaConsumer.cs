using Confluent.Kafka;
using Fanex.Logging;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.FingerPrint.Kafka.Handlers.Kafka.Handlers
{
    public interface IKafkaHandler<Tk, Tv>
    {
        Task HandleAsync(Tk key, Tv value);
    }

    public interface IKafkaConsumer<TKey, TValue> where TValue : class
    {
        Task Consume(string topic, CancellationToken stoppingToken);

        void Close();

        void Dispose();
    }

    public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
    {
        private readonly ConsumerConfig _config;
        private IKafkaHandler<TKey, TValue> _handler;
        private IConsumer<TKey, TValue> _consumer;

        private string _topic;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public KafkaConsumer(ConsumerConfig config, IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _config = config;
        }

        public async Task Consume(string topic, CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            _handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
            _consumer = new ConsumerBuilder<TKey, TValue>(_config).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
            _topic = topic;

            await Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        public void Close()
        {
            _consumer.Close();
        }

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
                catch (ConsumeException ex)
                {
                    LogService.Error(ex.Message, ex);

                    if (ex.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    LogService.Error(ex.Message, ex);
                    break;
                }
            }
        }
    }
}
