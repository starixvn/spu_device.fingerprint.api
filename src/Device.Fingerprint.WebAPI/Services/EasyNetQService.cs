using Device.FingerPrint.Queue;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using System;

namespace Device.Fingerprint.WebAPI.Services
{

    public interface IEasyNetQService
    {
        public void PushData<T>(T model);
    }

    public class EasyNetQService : IEasyNetQService
    {
        private EasyNetQConnector _easyNetQConnector;
        private readonly IConfiguration _configuration;
        private bool IgnoreMessage;
        public EasyNetQService(IConfiguration _configuration)
        {
            this._configuration = _configuration;

            InitialConnection();
        }

        private void InitialConnection()
        {

            _easyNetQConnector = new EasyNetQConnector(_configuration["RabbitMQ:Host"] + ":" + _configuration["RabbitMQ:Port"],
                                      _configuration["RabbitMQ:VirtualHost"],
                                      int.Parse(_configuration["RabbitMQ:PrefetchCount"].ToString()),
                                      _configuration["RabbitMQ:UserName"],
                                      _configuration["RabbitMQ:Password"]);

            _easyNetQConnector.CreateBus()
                    .DeclareExchange(_configuration["RabbitMQ:Exchange"])
                    .DeclareQueue(_configuration["RabbitMQ:Name"])
                    .DeclareRoutingKey(_configuration["RabbitMQ:RoutingKey"])
                    .Bind();

            IgnoreMessage = Convert.ToBoolean(_configuration["RabbitMQ:IgnoreMessage"]);
        }

        public void PushData<T>(T model)
        {
            if(!IgnoreMessage)
            {
                var message = new Message<T>(model, new MessageProperties() { DeliveryMode = 2, Priority = 9 });

                _easyNetQConnector.AdvanceBus.Publish(_easyNetQConnector.Exchange, _easyNetQConnector.RoutingKey, true, message);
            }           
        }
    }
}
