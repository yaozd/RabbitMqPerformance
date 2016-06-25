using System;

namespace RabbitMQ.NETClient.Customer
{
    public sealed class Singleton
    {
        private static readonly Lazy<Singleton> lazy =
            new Lazy<Singleton>(() => new Singleton());

        public static Singleton Instance
        {
            get { return lazy.Value; }
        }

        private RabbitMqCustomer _rabbitMqCustomer;

        private Singleton()
        {
            InitRabbitMqClients();
        }

        public RabbitMqCustomer MqCustomer()
        {
            if (_rabbitMqCustomer == null) SetRabbitMqCustomer();
            return _rabbitMqCustomer;
        }

        private void SetRabbitMqCustomer()
        {
            _rabbitMqCustomer = LoadRabbitMqCustomer();
        }

        private void InitRabbitMqClients()
        {
            _rabbitMqCustomer = LoadRabbitMqCustomer();
        }

        private RabbitMqCustomer LoadRabbitMqCustomer()
        {
            return new RabbitMqCustomer("10.48.251.74");
        }
    }
}