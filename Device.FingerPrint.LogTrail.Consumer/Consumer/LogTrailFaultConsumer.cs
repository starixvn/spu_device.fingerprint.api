namespace Device.FingerPrint.LogTrail.Consumer.Consumer
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FPS.Device.FingerPrint.LogTrail.Net.Standard.Client.Models;
    using MassTransit;

    public class LogTrailFaultConsumer : IConsumer<Fault<LogEventInfo>>
    {
        public Task Consume(ConsumeContext<Fault<LogEventInfo>> context)
        {
            Console.WriteLine("log trail faulted: " + context.Message.Exceptions.First().Message);

            return Task.CompletedTask;
        }
    }
}
