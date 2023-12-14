using Device.FingerPrint.Queue;
using EasyNetQ;
using Microsoft.Extensions.Configuration;

namespace Device.FingerPrint.PortalAPI.Services
{

    public interface IEasyNetQService
    {
        public void PushData<T>(T model);
    }

    public class EasyNetQService : IEasyNetQService
    {
        private readonly IConfiguration _configuration;

        public EasyNetQService(IConfiguration _configuration)
        {
            this._configuration = _configuration;
        }

        public void PushData<T>(T model)
        {
            var message = new Message<T>(model, new MessageProperties() { DeliveryMode = 2, Priority = 9 });

            var easyNetQConnector = RabbitMQBus();
            easyNetQConnector.AdvanceBus.Publish(easyNetQConnector.Exchange, easyNetQConnector.RoutingKey, true, message);
        }

        private EasyNetQConnector RabbitMQBus()
        {
            var _easyNetQConnector = new EasyNetQConnector(_configuration["RabbitMQ:Host"] + ":" + _configuration["RabbitMQ:Port"],
                                      _configuration["RabbitMQ:VirtualHost"],
                                      int.Parse(_configuration["RabbitMQ:PrefetchCount"].ToString()),
                                      _configuration["RabbitMQ:UserName"],
                                      _configuration["RabbitMQ:Password"]);

            _easyNetQConnector.CreateBus()
                    .DeclareExchange(_configuration["RabbitMQ:Exchange"])
                    .DeclareQueue(_configuration["RabbitMQ:Name"])
                    .DeclareRoutingKey(_configuration["RabbitMQ:RoutingKey"])
                    .Bind();
            return _easyNetQConnector;
        }
    }
}
