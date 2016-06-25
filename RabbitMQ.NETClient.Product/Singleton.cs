using System;
using RabbitMQ.Client;

namespace RabbitMQ.NETClient.Product
{
    public sealed class Singleton
    {
        private static readonly Lazy<Singleton> lazy =
          new Lazy<Singleton>(() => new Singleton());
        public static Singleton Instance
        {
            get { return lazy.Value; }
        }

        private RabbitMqProduct _rabbitMqProduct;
        private Singleton()
        {
            InitRabbitMqClients();
        }

        public RabbitMqProduct MqProduct()
        {
            if (_rabbitMqProduct == null) SetRabbitMqProduct();
            return _rabbitMqProduct;
        }

        private void SetRabbitMqProduct()
        {
            _rabbitMqProduct = LoadRabbitMqProduct();
        }

        private void InitRabbitMqClients()
        {
            _rabbitMqProduct = LoadRabbitMqProduct();
        }

        private RabbitMqProduct LoadRabbitMqProduct()
        {
            return new RabbitMqProduct("10.48.251.74");
        }
    }
}