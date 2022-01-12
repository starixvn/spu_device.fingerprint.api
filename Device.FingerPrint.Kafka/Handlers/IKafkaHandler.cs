using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Device.FingerPrint.Kafka.Handlers
{
    public interface IKafkaHandler : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken);
    }
}
