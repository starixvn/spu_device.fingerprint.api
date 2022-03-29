using Device.FingerPrint.Model;
using Device.FingerPrint.Queue;
using EasyNetQ;
using Microsoft.Extensions.Configuration;

namespace Device.Fingerprint.WebAPI.Services
{

    public interface IEasyNetQService
    {
        public void PushServerTransactionToMonitor(TransactionModel model);

        public void PushClientTransactionToMonitor(ClientTransactionModel model);
    }

    public class EasyNetQService : IEasyNetQService
    {
        private EasyNetQConnector _rabbitMQMonitor;
        private readonly IConfiguration _configuration;

        public EasyNetQService(IConfiguration _configuration)
        {
            this._configuration = _configuration;
            MonitorMessageBus();
        }

        private void MonitorMessageBus()
        {

            _rabbitMQMonitor = new EasyNetQConnector(_configuration["RabbitMQ:Host"] + ":" + _configuration["RabbitMQ:Port"],
                                      _configuration["RabbitMQ:VirtualHost"],
                                      int.Parse(_configuration["RabbitMQ:PrefetchCount"].ToString()),
                                      _configuration["RabbitMQ:UserName"],
                                      _configuration["RabbitMQ:Password"]);

            _rabbitMQMonitor.CreateBus()
                    .DeclareExchange(_configuration["RabbitMQ:Exchange"])
                    .DeclareQueue(_configuration["RabbitMQ:Name"])
                    .DeclareRoutingKey(_configuration["RabbitMQ:RoutingKey"])
                    .Bind();
        }

        public void PushClientTransactionToMonitor(ClientTransactionModel model)
        {
            var message = new Message<ClientTransactionModel>(model, new MessageProperties() { DeliveryMode = 2, Priority = 9 });

            _rabbitMQMonitor.AdvanceBus.Publish(_rabbitMQMonitor.Exchange, _rabbitMQMonitor.RoutingKey, true, message);
        }

        public void PushServerTransactionToMonitor(TransactionModel model)
        {
            var message = new Message<TransactionModel>(model, new MessageProperties() { DeliveryMode = 2, Priority = 9 });

            _rabbitMQMonitor.AdvanceBus.Publish(_rabbitMQMonitor.Exchange, _rabbitMQMonitor.RoutingKey, true, message);
        }
    }
}
