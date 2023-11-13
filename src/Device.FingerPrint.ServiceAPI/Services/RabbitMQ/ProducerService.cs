namespace Device.FingerPrint.ServiceAPI.Services.RabbitMQ
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MassTransit;

    public interface IProducerService
    {
        Task SendAsync<T>(T data, string address);

        Task SendBatchAsync<T>(IEnumerable<T> datas, string address) where T : class;
    }
    public class ProducerService: IProducerService
    {
        private readonly IBus bus;

        public ProducerService(IBus bus)
        {
            this.bus = bus;
        }

        public async Task SendAsync<T>(T data, string address)
        {
            var endpoint = await bus.GetSendEndpoint(new Uri(address));
            await endpoint.Send(data, new CancellationToken());
        }

        public async Task SendBatchAsync<T>(IEnumerable<T> datas, string address) where T : class
        {
            var endpoint = await bus.GetSendEndpoint(new Uri(address));
            await endpoint.SendBatch(datas, new CancellationToken());
        }
    }
}
