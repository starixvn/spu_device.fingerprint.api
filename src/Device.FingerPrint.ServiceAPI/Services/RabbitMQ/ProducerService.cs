namespace Device.FingerPrint.ServiceAPI.Services.RabbitMQ
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using MassTransit;

    public interface IProducerService
    {
        Task SendAsync<T>(T data, string address);
    }
    public class ProducerService: IProducerService
    {
        private readonly IBus bus;
        private readonly ILogger logger;

        public ProducerService(IBus bus, ILogger logger)
        {
            this.bus = bus;
            this.logger = logger;
        }

        public async Task SendAsync<T>(T data, string address)
        {
            var endpoint = await bus.GetSendEndpoint(new Uri(address));
            await endpoint.Send(data, new CancellationToken());
        }
    }
}
